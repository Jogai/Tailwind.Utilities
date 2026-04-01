using System.Globalization;
using System.Text.RegularExpressions;
using Stu.Parsing;

namespace Stu.Transformation;

/// <summary>
/// Converts unitless line-height values to px.
/// When a rule contains both font-size (in px) and a unitless line-height,
/// the line-height is computed as: line-height * font-size.
/// For standalone unitless line-heights (no font-size in the rule),
/// the base font size is used.
/// </summary>
public partial class LineHeightConverter
{
    private readonly int _baseFontSize;

    public LineHeightConverter(int baseFontSize = 16)
    {
        _baseFontSize = baseFontSize;
    }

    public void ConvertRule(CssRule rule)
    {
        // Find font-size declaration if present (for paired text-* rules)
        double? fontSizePx = null;
        foreach (var decl in rule.Declarations)
        {
            if (decl.Property == "font-size")
            {
                fontSizePx = ParsePxValue(decl.Value);
                break;
            }
        }

        for (var i = 0; i < rule.Declarations.Count; i++)
        {
            var decl = rule.Declarations[i];
            if (decl.Property != "line-height")
                continue;

            var value = decl.Value.Trim();

            // Skip values that already have units or are keywords
            if (value.EndsWith("px") || value.EndsWith("em") || value.EndsWith("rem")
                || value.EndsWith("%") || value == "normal" || value == "inherit"
                || value == "initial" || value == "unset")
                continue;

            // Try parse as a unitless number
            if (!double.TryParse(value, CultureInfo.InvariantCulture, out var ratio))
                continue;

            // Compute px value: ratio * (paired font-size or base font size)
            var reference = fontSizePx ?? _baseFontSize;
            var px = ratio * reference;

            if (px == 0)
            {
                rule.Declarations[i] = decl.WithValue("0");
            }
            else
            {
                var rounded = Math.Round(px, 2);
                var formatted = rounded == Math.Floor(rounded)
                    ? ((int)rounded).ToString(CultureInfo.InvariantCulture)
                    : rounded.ToString("G", CultureInfo.InvariantCulture);
                rule.Declarations[i] = decl.WithValue($"{formatted}px");
            }
        }
    }

    private static double? ParsePxValue(string value)
    {
        var match = PxValueRegex().Match(value.Trim());
        if (match.Success && double.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var px))
            return px;
        return null;
    }

    [GeneratedRegex(@"^(-?\d+\.?\d*)px$")]
    private static partial Regex PxValueRegex();
}
