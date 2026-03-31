using Stu.Parsing;

namespace Stu.Transformation;

/// <summary>
/// Removes CSS rules whose selectors contain arbitrary values (bracket notation).
/// e.g., .w-\[100px\], .bg-\[#ff0000\]
/// </summary>
public static class ArbitraryValueFilter
{
    public static List<CssRule> Filter(List<CssRule> rules)
    {
        return rules.Where(r => !HasArbitraryValue(r.Selector)).ToList();
    }

    private static bool HasArbitraryValue(string selector)
    {
        // Tailwind escapes brackets in selectors: .w-\[100px\]
        return selector.Contains("\\[") || selector.Contains("\\]");
    }
}
