using System.Globalization;
using System.Text.RegularExpressions;
using Stu.Parsing;

namespace Stu.Transformation;

/// <summary>
/// Resolves CSS custom property references (var(--name)) by substituting
/// values from the theme block (:root/:host). Also evaluates simple
/// calc() expressions after substitution.
/// </summary>
public partial class CssVariableResolver
{
    private readonly Dictionary<string, string> _variables = new(StringComparer.Ordinal);
    private readonly int _baseFontSize;

    public CssVariableResolver(int baseFontSize = 16)
    {
        _baseFontSize = baseFontSize;
    }

    /// <summary>
    /// Extracts variable definitions from rules with :root or :host selectors,
    /// removes those rules from the list, and returns them separately.
    /// </summary>
    public List<CssRule> ExtractAndRemoveThemeRules(List<CssRule> rules)
    {
        var remaining = new List<CssRule>();

        foreach (var rule in rules)
        {
            if (IsThemeSelector(rule.Selector))
            {
                foreach (var decl in rule.Declarations)
                {
                    if (decl.Property.StartsWith("--"))
                        _variables[decl.Property] = decl.Value;
                }
            }
            else
            {
                remaining.Add(rule);
            }
        }

        // Resolve variables that reference other variables (e.g. --default-font-family: var(--font-sans))
        ResolveInternalReferences();

        return remaining;
    }

    /// <summary>
    /// Resolves all var() references in declaration values using the extracted theme variables.
    /// Also evaluates calc() expressions where possible.
    /// </summary>
    public string Resolve(string value)
    {
        if (!value.Contains("var("))
            return TryEvaluateCalc(value);

        // Iteratively resolve var() from innermost out (handles nesting)
        var resolved = value;
        var maxIterations = 10;
        while (resolved.Contains("var(") && maxIterations-- > 0)
        {
            resolved = VarRegex().Replace(resolved, match =>
            {
                var varName = match.Groups[1].Value.Trim();
                var fallback = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

                if (_variables.TryGetValue(varName, out var varValue))
                    return varValue;

                // Use fallback if available
                if (fallback != null)
                    return fallback.TrimStart(' ', ',');

                // Can't resolve — return as-is
                return match.Value;
            });
        }

        return TryEvaluateCalc(resolved);
    }

    private void ResolveInternalReferences()
    {
        // Multiple passes to handle chains like --a: var(--b) where --b: var(--c)
        for (var pass = 0; pass < 5; pass++)
        {
            var changed = false;
            foreach (var key in _variables.Keys.ToList())
            {
                var value = _variables[key];
                if (!value.Contains("var(")) continue;

                var resolved = VarRegex().Replace(value, match =>
                {
                    var varName = match.Groups[1].Value.Trim();
                    var fallback = match.Groups[2].Success ? match.Groups[2].Value.Trim().TrimStart(' ', ',') : null;

                    if (_variables.TryGetValue(varName, out var v))
                        return v;
                    if (fallback != null)
                        return fallback;
                    return match.Value;
                });

                if (resolved != value)
                {
                    _variables[key] = resolved;
                    changed = true;
                }
            }
            if (!changed) break;
        }

        // Evaluate calc() in variable values too
        foreach (var key in _variables.Keys.ToList())
        {
            _variables[key] = TryEvaluateCalc(_variables[key]);
        }
    }

    private string TryEvaluateCalc(string value)
    {
        if (!value.Contains("calc("))
            return value;

        return CalcRegex().Replace(value, match =>
        {
            var expr = match.Groups[1].Value.Trim();
            var result = EvaluateCalcExpression(expr);
            return result ?? match.Value;
        });
    }

    /// <summary>
    /// Evaluates simple calc expressions: multiplication, division, addition, subtraction.
    /// Handles expressions like "4px * 16", "1 / 0.75", "2 / 12 * 100%".
    /// Returns null if the expression can't be evaluated.
    /// </summary>
    private static string? EvaluateCalcExpression(string expr)
    {
        // If there are still var() references, can't evaluate
        if (expr.Contains("var("))
            return null;

        // Extract the unit if present (px, %, etc.)
        // We'll track units through the calculation
        var tokens = TokenizeCalcExpression(expr);
        if (tokens == null) return null;

        var result = EvaluateTokens(tokens, out var unit);
        if (result == null) return null;

        var val = result.Value;

        // Format the result
        if (val == 0)
            return unit == "%" ? "0%" : "0";

        var formatted = FormatNumber(val);
        return $"{formatted}{unit}";
    }

    private static string FormatNumber(double val)
    {
        // Format cleanly, avoiding trailing zeros
        if (val == Math.Floor(val) && Math.Abs(val) < 1e10)
            return ((long)val).ToString(CultureInfo.InvariantCulture);

        var formatted = val.ToString("G10", CultureInfo.InvariantCulture);
        // Round to reasonable precision for CSS
        var rounded = Math.Round(val, 4);
        var roundedStr = rounded.ToString("G10", CultureInfo.InvariantCulture);
        if (roundedStr.Length < formatted.Length)
            formatted = roundedStr;

        return formatted;
    }

    private static List<CalcToken>? TokenizeCalcExpression(string expr)
    {
        var tokens = new List<CalcToken>();
        var i = 0;

        while (i < expr.Length)
        {
            // Skip whitespace
            while (i < expr.Length && char.IsWhiteSpace(expr[i])) i++;
            if (i >= expr.Length) break;

            var ch = expr[i];

            // Operator
            if (ch is '+' or '*' or '/')
            {
                tokens.Add(new CalcToken(CalcTokenType.Operator, ch.ToString(), 0, ""));
                i++;
                continue;
            }

            // Minus can be operator or start of negative number
            if (ch == '-')
            {
                // It's an operator if the previous token was a number
                if (tokens.Count > 0 && tokens[^1].Type == CalcTokenType.Number)
                {
                    tokens.Add(new CalcToken(CalcTokenType.Operator, "-", 0, ""));
                    i++;
                    continue;
                }
            }

            // Number (possibly negative, possibly with unit)
            if (ch == '-' || ch == '.' || char.IsDigit(ch))
            {
                var start = i;
                if (expr[i] == '-') i++;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;

                var numStr = expr[start..i];
                if (!double.TryParse(numStr, CultureInfo.InvariantCulture, out var num))
                    return null;

                // Check for unit
                var unitStart = i;
                while (i < expr.Length && char.IsLetter(expr[i]) || (i < expr.Length && expr[i] == '%')) i++;
                var unit = expr[unitStart..i];

                tokens.Add(new CalcToken(CalcTokenType.Number, "", num, unit));
                continue;
            }

            // Unknown character — bail
            return null;
        }

        return tokens.Count > 0 ? tokens : null;
    }

    private static double? EvaluateTokens(List<CalcToken> tokens, out string unit)
    {
        unit = "";

        if (tokens.Count == 0) return null;

        // Simple left-to-right evaluation respecting operator precedence:
        // First pass: handle * and /
        var values = new List<(double val, string unit)>();
        var ops = new List<string>();

        // Extract numbers and operators
        foreach (var token in tokens)
        {
            if (token.Type == CalcTokenType.Number)
                values.Add((token.Value, token.Unit));
            else
                ops.Add(token.Text);
        }

        if (values.Count == 0) return null;
        if (values.Count != ops.Count + 1) return null;

        // First pass: multiply and divide
        var resultValues = new List<(double val, string unit)> { values[0] };
        var resultOps = new List<string>();

        for (var i = 0; i < ops.Count; i++)
        {
            if (ops[i] is "*" or "/")
            {
                var left = resultValues[^1];
                var right = values[i + 1];

                double val;
                string resultUnit;

                if (ops[i] == "*")
                {
                    val = left.val * right.val;
                    resultUnit = left.unit != "" ? left.unit : right.unit;
                }
                else
                {
                    if (right.val == 0) return null;
                    val = left.val / right.val;
                    // Division: if both have same unit, result is unitless; otherwise keep left's unit
                    if (left.unit == right.unit && left.unit != "")
                        resultUnit = "";
                    else
                        resultUnit = left.unit != "" ? left.unit : "";
                }

                resultValues[^1] = (val, resultUnit);
            }
            else
            {
                resultValues.Add(values[i + 1]);
                resultOps.Add(ops[i]);
            }
        }

        // Second pass: add and subtract
        var final = resultValues[0].val;
        unit = resultValues[0].unit;

        for (var i = 0; i < resultOps.Count; i++)
        {
            var right = resultValues[i + 1];
            // Inherit unit from whichever has one
            if (unit == "" && right.unit != "") unit = right.unit;

            final = resultOps[i] switch
            {
                "+" => final + right.val,
                "-" => final - right.val,
                _ => final
            };
        }

        return final;
    }

    private static bool IsThemeSelector(string selector)
    {
        // Tailwind v4 puts theme in ":root, :host" or just ":root"
        return selector.Contains(":root") || selector.Contains(":host");
    }

    /// <summary>
    /// Matches var(--name) or var(--name, fallback).
    /// The fallback group captures everything after the first comma.
    /// Uses balanced parenthesis matching for nested var() calls.
    /// </summary>
    [GeneratedRegex(@"var\(\s*(--[\w-]+)\s*(?:,\s*([^)]*(?:\([^)]*\)[^)]*)*))?\s*\)")]
    private static partial Regex VarRegex();

    [GeneratedRegex(@"calc\(([^()]*(?:\([^()]*\)[^()]*)*)\)")]
    private static partial Regex CalcRegex();

    private enum CalcTokenType { Number, Operator }
    private record CalcToken(CalcTokenType Type, string Text, double Value, string Unit);
}
