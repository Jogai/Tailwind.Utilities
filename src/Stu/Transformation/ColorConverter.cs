using System.Globalization;
using System.Text.RegularExpressions;

namespace Stu.Transformation;

/// <summary>
/// Converts color function values (oklch, oklab, rgb, hsl) to hex notation.
/// Tailwind v4 primarily uses oklch().
/// </summary>
public partial class ColorConverter
{
    public string Convert(string value)
    {
        // Process oklch() first (most common in Tailwind v4)
        value = OklchRegex().Replace(value, ConvertOklch);

        // Process oklab()
        value = OklabRegex().Replace(value, ConvertOklab);

        // Process hsl/hsla()
        value = HslRegex().Replace(value, ConvertHsl);

        // Process rgb/rgba() with modern space syntax
        value = RgbRegex().Replace(value, ConvertRgb);

        return value;
    }

    // --- oklch ---

    private static string ConvertOklch(Match match)
    {
        var l = ParseComponent(match.Groups[1].Value);
        var c = ParseComponent(match.Groups[2].Value);
        var h = ParseComponent(match.Groups[3].Value);
        var alpha = match.Groups[4].Success ? ParseAlpha(match.Groups[4].Value) : 1.0;

        // Handle "none" keyword (Tailwind sometimes uses this)
        if (double.IsNaN(l)) l = 0;
        if (double.IsNaN(c)) c = 0;
        if (double.IsNaN(h)) h = 0;

        var (r, g, b) = OklchToSrgb(l, c, h);
        return ToHex(r, g, b, alpha);
    }

    internal static (double r, double g, double b) OklchToSrgb(double l, double c, double h)
    {
        // OKLCH to OKLab (polar to Cartesian)
        var hRad = h * Math.PI / 180.0;
        var a = c * Math.Cos(hRad);
        var bLab = c * Math.Sin(hRad);

        return OklabToSrgb(l, a, bLab);
    }

    // --- oklab ---

    private static string ConvertOklab(Match match)
    {
        var l = ParseComponent(match.Groups[1].Value);
        var a = ParseComponent(match.Groups[2].Value);
        var b = ParseComponent(match.Groups[3].Value);
        var alpha = match.Groups[4].Success ? ParseAlpha(match.Groups[4].Value) : 1.0;

        if (double.IsNaN(l)) l = 0;
        if (double.IsNaN(a)) a = 0;
        if (double.IsNaN(b)) b = 0;

        var (r, g, bOut) = OklabToSrgb(l, a, b);
        return ToHex(r, g, bOut, alpha);
    }

    internal static (double r, double g, double b) OklabToSrgb(double l, double a, double bVal)
    {
        // OKLab to LMS (cube root space)
        var l_ = l + 0.3963377774 * a + 0.2158037573 * bVal;
        var m_ = l - 0.1055613458 * a - 0.0638541728 * bVal;
        var s_ = l - 0.0894841775 * a - 1.2914855480 * bVal;

        // Cube to get LMS
        var lLms = l_ * l_ * l_;
        var mLms = m_ * m_ * m_;
        var sLms = s_ * s_ * s_;

        // LMS to linear sRGB
        var r = +4.0767416621 * lLms - 3.3077115913 * mLms + 0.2309699292 * sLms;
        var g = -1.2684380046 * lLms + 2.6097574011 * mLms - 0.3413193965 * sLms;
        var b = -0.0041960863 * lLms - 0.7034186147 * mLms + 1.7076147010 * sLms;

        // Linear sRGB to sRGB (gamma)
        r = LinearToSrgb(r);
        g = LinearToSrgb(g);
        b = LinearToSrgb(b);

        return (Clamp01(r), Clamp01(g), Clamp01(b));
    }

    // --- hsl ---

    private static string ConvertHsl(Match match)
    {
        var h = ParseComponent(match.Groups[1].Value);
        var s = ParsePercentage(match.Groups[2].Value);
        var l = ParsePercentage(match.Groups[3].Value);
        var alpha = match.Groups[4].Success ? ParseAlpha(match.Groups[4].Value) : 1.0;

        var (r, g, b) = HslToSrgb(h, s, l);
        return ToHex(r, g, b, alpha);
    }

    internal static (double r, double g, double b) HslToSrgb(double h, double s, double l)
    {
        h = ((h % 360) + 360) % 360; // normalize
        s = Math.Clamp(s, 0, 1);
        l = Math.Clamp(l, 0, 1);

        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        var m = l - c / 2;

        var (r1, g1, b1) = h switch
        {
            < 60 => (c, x, 0.0),
            < 120 => (x, c, 0.0),
            < 180 => (0.0, c, x),
            < 240 => (0.0, x, c),
            < 300 => (x, 0.0, c),
            _ => (c, 0.0, x)
        };

        return (Clamp01(r1 + m), Clamp01(g1 + m), Clamp01(b1 + m));
    }

    // --- rgb ---

    private static string ConvertRgb(Match match)
    {
        var r = ParseRgbComponent(match.Groups[1].Value);
        var g = ParseRgbComponent(match.Groups[2].Value);
        var b = ParseRgbComponent(match.Groups[3].Value);
        var alpha = match.Groups[4].Success ? ParseAlpha(match.Groups[4].Value) : 1.0;

        return ToHex(r / 255.0, g / 255.0, b / 255.0, alpha);
    }

    // --- Helpers ---

    private static double LinearToSrgb(double c)
    {
        if (c <= 0.0031308)
            return 12.92 * c;
        return 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
    }

    private static double Clamp01(double v) => Math.Clamp(v, 0.0, 1.0);

    private static string ToHex(double r, double g, double b, double alpha)
    {
        var ri = (int)Math.Round(r * 255);
        var gi = (int)Math.Round(g * 255);
        var bi = (int)Math.Round(b * 255);

        if (alpha >= 0.999)
            return $"#{ri:X2}{gi:X2}{bi:X2}";

        var ai = (int)Math.Round(alpha * 255);
        return $"#{ri:X2}{gi:X2}{bi:X2}{ai:X2}";
    }

    private static double ParseComponent(string value)
    {
        value = value.Trim();
        if (string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
            return double.NaN;

        if (value.EndsWith('%'))
        {
            var pct = double.Parse(value[..^1], CultureInfo.InvariantCulture);
            return pct / 100.0;
        }

        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    private static double ParsePercentage(string value)
    {
        value = value.Trim().TrimEnd('%');
        return double.Parse(value, CultureInfo.InvariantCulture) / 100.0;
    }

    private static double ParseAlpha(string value)
    {
        value = value.Trim();
        if (value.EndsWith('%'))
            return double.Parse(value[..^1], CultureInfo.InvariantCulture) / 100.0;
        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    private static double ParseRgbComponent(string value)
    {
        value = value.Trim();
        if (value.EndsWith('%'))
            return double.Parse(value[..^1], CultureInfo.InvariantCulture) / 100.0 * 255.0;
        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    // oklch(L C H) or oklch(L C H / alpha)
    [GeneratedRegex(@"oklch\(\s*([\d.]+%?|none)\s+([\d.]+%?|none)\s+([\d.]+|none)\s*(?:/\s*([\d.]+%?))?\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex OklchRegex();

    // oklab(L a b) or oklab(L a b / alpha)
    [GeneratedRegex(@"oklab\(\s*([\d.]+%?|none)\s+([-\d.]+%?|none)\s+([-\d.]+%?|none)\s*(?:/\s*([\d.]+%?))?\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex OklabRegex();

    // hsl(H S L) or hsl(H, S%, L%) or hsla(...)
    [GeneratedRegex(@"hsla?\(\s*([\d.]+)\s*[,\s]\s*([\d.]+)%?\s*[,\s]\s*([\d.]+)%?\s*(?:[,/]\s*([\d.]+%?))?\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex HslRegex();

    // rgb(R G B) or rgb(R, G, B) or rgba(...)
    [GeneratedRegex(@"rgba?\(\s*([\d.]+%?)\s*[,\s]\s*([\d.]+%?)\s*[,\s]\s*([\d.]+%?)\s*(?:[,/]\s*([\d.]+%?))?\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex RgbRegex();
}
