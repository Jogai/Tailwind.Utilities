using System.Text.RegularExpressions;

namespace Stu.Parsing;

/// <summary>
/// Regex-based CSS parser for Tailwind's machine-generated, flat CSS output.
/// Strips @layer wrappers, @property, @keyframes, and extracts style rules.
/// </summary>
public partial class CssParser
{
    public List<CssRule> Parse(string css)
    {
        // Step 1: Remove @keyframes blocks (they can contain nested braces)
        css = RemoveKeyframes(css);

        // Step 2: Remove @property blocks and standalone at-rules (e.g. @charset)
        css = PropertyBlockRegex().Replace(css, "");
        css = StandaloneAtRuleRegex().Replace(css, "");

        // Step 3: Unwrap @layer blocks — keep inner content, discard the wrapper
        css = UnwrapLayers(css);

        // Step 4: Remove @media and @supports blocks entirely
        css = RemoveMediaBlocks(css);
        css = RemoveSupportsBlocks(css);

        // Step 5: Extract style rules
        var rules = new List<CssRule>();
        foreach (Match match in StyleRuleRegex().Matches(css))
        {
            var selector = match.Groups[1].Value.Trim();
            var body = match.Groups[2].Value.Trim();

            if (string.IsNullOrWhiteSpace(selector) || string.IsNullOrWhiteSpace(body))
                continue;

            // Skip remaining at-rules that weren't caught
            if (selector.StartsWith('@'))
                continue;

            var declarations = ParseDeclarations(body);
            if (declarations.Count > 0)
                rules.Add(new CssRule(selector, declarations));
        }

        return rules;
    }

    private static List<CssDeclaration> ParseDeclarations(string body)
    {
        var declarations = new List<CssDeclaration>();

        foreach (var part in SplitDeclarations(body))
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
                continue;

            var property = trimmed[..colonIndex].Trim();
            var value = trimmed[(colonIndex + 1)..].Trim();

            // Remove trailing semicolons
            if (value.EndsWith(';'))
                value = value[..^1].Trim();

            if (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(value)
                && !property.StartsWith("-webkit-") && !property.StartsWith("-moz-"))
                declarations.Add(new CssDeclaration(property, value));
        }

        return declarations;
    }

    /// <summary>
    /// Split declarations by semicolons, respecting parentheses depth
    /// (so we don't split inside function calls like oklch(...)).
    /// </summary>
    private static IEnumerable<string> SplitDeclarations(string body)
    {
        var depth = 0;
        var start = 0;

        for (var i = 0; i < body.Length; i++)
        {
            switch (body[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ';' when depth == 0:
                    yield return body[start..i];
                    start = i + 1;
                    break;
            }
        }

        if (start < body.Length)
            yield return body[start..];
    }

    private static string RemoveKeyframes(string css)
    {
        // @keyframes can have nested braces, so we need to count them
        var result = new System.Text.StringBuilder();
        var i = 0;
        while (i < css.Length)
        {
            var kfIndex = css.IndexOf("@keyframes", i, StringComparison.Ordinal);
            if (kfIndex < 0)
            {
                result.Append(css, i, css.Length - i);
                break;
            }

            result.Append(css, i, kfIndex - i);

            // Find the opening brace
            var braceStart = css.IndexOf('{', kfIndex);
            if (braceStart < 0) break;

            // Count braces to find the matching close
            var depth = 1;
            var j = braceStart + 1;
            while (j < css.Length && depth > 0)
            {
                if (css[j] == '{') depth++;
                else if (css[j] == '}') depth--;
                j++;
            }

            i = j;
        }

        return result.ToString();
    }

    private static string UnwrapLayers(string css)
    {
        // Replace @layer <name> { ... } with just the inner content
        // Need brace counting for nesting
        var result = css;
        bool found;

        do
        {
            found = false;
            var layerMatch = Regex.Match(result, @"@layer\s+[\w-]+\s*\{");
            if (!layerMatch.Success) break;

            found = true;
            var start = layerMatch.Index;
            var braceStart = start + layerMatch.Length - 1;

            var depth = 1;
            var j = braceStart + 1;
            while (j < result.Length && depth > 0)
            {
                if (result[j] == '{') depth++;
                else if (result[j] == '}') depth--;
                j++;
            }

            // Extract inner content (between outer braces)
            var inner = result[(braceStart + 1)..(j - 1)];
            result = result[..start] + inner + result[j..];
        } while (found);

        return result;
    }

    private static string RemoveMediaBlocks(string css)
    {
        var result = new System.Text.StringBuilder();
        var i = 0;
        while (i < css.Length)
        {
            var mediaIndex = css.IndexOf("@media", i, StringComparison.Ordinal);
            if (mediaIndex < 0)
            {
                result.Append(css, i, css.Length - i);
                break;
            }

            result.Append(css, i, mediaIndex - i);

            var braceStart = css.IndexOf('{', mediaIndex);
            if (braceStart < 0) break;

            var depth = 1;
            var j = braceStart + 1;
            while (j < css.Length && depth > 0)
            {
                if (css[j] == '{') depth++;
                else if (css[j] == '}') depth--;
                j++;
            }

            i = j;
        }

        return result.ToString();
    }

    private static string RemoveSupportsBlocks(string css)
    {
        var result = new System.Text.StringBuilder();
        var i = 0;
        while (i < css.Length)
        {
            var idx = css.IndexOf("@supports", i, StringComparison.Ordinal);
            if (idx < 0)
            {
                result.Append(css, i, css.Length - i);
                break;
            }

            result.Append(css, i, idx - i);

            var braceStart = css.IndexOf('{', idx);
            if (braceStart < 0) break;

            var depth = 1;
            var j = braceStart + 1;
            while (j < css.Length && depth > 0)
            {
                if (css[j] == '{') depth++;
                else if (css[j] == '}') depth--;
                j++;
            }

            i = j;
        }

        return result.ToString();
    }

    [GeneratedRegex(@"@property\s+[^{]+\{[^}]*\}", RegexOptions.Singleline)]
    private static partial Regex PropertyBlockRegex();

    /// <summary>
    /// Matches standalone at-rules that end with a semicolon (no braces), e.g. @charset, @import, @namespace.
    /// </summary>
    [GeneratedRegex(@"@(?:charset|import|namespace)\s+[^;]+;", RegexOptions.IgnoreCase)]
    private static partial Regex StandaloneAtRuleRegex();

    /// <summary>
    /// Matches a CSS rule: selector { declarations }
    /// The selector is everything before { and declarations are everything inside { }.
    /// Only matches single-level braces (no nesting).
    /// </summary>
    [GeneratedRegex(@"([^{}]+?)\{([^{}]+)\}", RegexOptions.Singleline)]
    private static partial Regex StyleRuleRegex();
}
