using System.Globalization;
using System.Text.RegularExpressions;

namespace Stu.Transformation;

/// <summary>
/// Converts rem and em units to px based on a configurable base font size.
/// </summary>
public partial class UnitConverter
{
    private readonly int _baseFontSize;

    public UnitConverter(int baseFontSize = 16)
    {
        _baseFontSize = baseFontSize;
    }

    public string Convert(string value)
    {
        value = RemRegex().Replace(value, match =>
        {
            var rem = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var px = rem * _baseFontSize;
            return FormatPx(px);
        });

        value = EmRegex().Replace(value, match =>
        {
            var em = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var px = em * _baseFontSize;
            return FormatPx(px);
        });

        return value;
    }

    private static string FormatPx(double px)
    {
        if (px == 0) return "0";

        // Format cleanly: 8px, 0.5px, etc.
        var formatted = px.ToString("G", CultureInfo.InvariantCulture);
        return $"{formatted}px";
    }

    // Match rem values: 0.5rem, 1rem, -0.25rem, .5rem, 16rem
    // Negative lookbehind to avoid matching inside function names
    [GeneratedRegex(@"(-?\d*\.?\d+)rem\b")]
    private static partial Regex RemRegex();

    // Match em values, but not "rem"
    [GeneratedRegex(@"(?<!r)(-?\d*\.?\d+)em\b")]
    private static partial Regex EmRegex();
}
