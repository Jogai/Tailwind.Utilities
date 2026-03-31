using System.Text.RegularExpressions;
using Stu.Parsing;

namespace Stu.Transformation;

/// <summary>
/// Removes CSS rules whose selectors contain pseudo-classes, pseudo-elements,
/// or other variant indicators that shouldn't be in the output.
/// </summary>
public static partial class VariantFilter
{
    public static List<CssRule> Filter(List<CssRule> rules)
    {
        return rules.Where(r => !IsVariantSelector(r.Selector)).ToList();
    }

    private static bool IsVariantSelector(string selector)
    {
        // Vendor-prefixed pseudo-elements (e.g. ::-webkit-scrollbar)
        if (selector.Contains("::-webkit-"))
            return true;

        // Pseudo-classes and pseudo-elements
        if (PseudoRegex().IsMatch(selector))
            return true;

        // Attribute selectors used as state (e.g. [open], [disabled])
        if (selector.Contains('[') && !selector.Contains("\\["))
            return true;

        // Combinators targeting children (stacking): > ~ +
        // Allow the root selector itself but strip things like .group:hover .group-hover\:xxx
        if (CombinatorRegex().IsMatch(selector))
            return true;

        return false;
    }

    [GeneratedRegex(@"::?(?:hover|focus|focus-within|focus-visible|active|visited|disabled|checked|indeterminate|required|valid|invalid|empty|first-child|last-child|first-of-type|last-of-type|only-child|only-of-type|nth-child|nth-of-type|placeholder|before|after|selection|file-selector-button|backdrop|marker|first-line|first-letter)", RegexOptions.IgnoreCase)]
    private static partial Regex PseudoRegex();

    [GeneratedRegex(@"\s[>~+]\s|\s[>~+]$|^[>~+]\s")]
    private static partial Regex CombinatorRegex();
}
