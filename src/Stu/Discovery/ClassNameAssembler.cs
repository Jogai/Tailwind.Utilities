using Stu.Config;

namespace Stu.Discovery;

/// <summary>
/// Combines discovered functional utility prefixes with known value scales
/// to produce the full set of candidate class names for @source inline().
/// </summary>
public class ClassNameAssembler
{
    private readonly IReadOnlyList<string> _colorFamilies;

    // Spacing scale values used by Tailwind's --spacing multiplier
    private static readonly string[] SpacingScale =
    {
        "0", "px", "0.5", "1", "1.5", "2", "2.5", "3", "3.5", "4", "5", "6", "7", "8",
        "9", "10", "11", "12", "14", "16", "20", "24", "28", "32", "36", "40", "44",
        "48", "52", "56", "60", "64", "72", "80", "96"
    };

    private static readonly string[] FractionScale =
    {
        "1/2", "1/3", "2/3", "1/4", "2/4", "3/4", "1/5", "2/5", "3/5", "4/5",
        "1/6", "2/6", "3/6", "4/6", "5/6", "1/12", "2/12", "3/12", "4/12",
        "5/12", "6/12", "7/12", "8/12", "9/12", "10/12", "11/12"
    };

    private static readonly string[] OpacityScale =
    {
        "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
        "55", "60", "65", "70", "75", "80", "85", "90", "95", "100"
    };

    // Functional prefixes that use the spacing scale
    private static readonly HashSet<string> SpacingPrefixes = new(StringComparer.Ordinal)
    {
        "p", "px", "py", "pt", "pr", "pb", "pl", "ps", "pe", "pbs", "pbe",
        "m", "mx", "my", "mt", "mr", "mb", "ml", "ms", "me", "mbs", "mbe",
        "size", "w", "min-w", "max-w", "h", "min-h", "max-h",
        "inline", "min-inline", "max-inline", "block", "min-block", "max-block",
        "inset", "inset-x", "inset-y", "inset-s", "inset-e", "inset-bs", "inset-be",
        "top", "right", "bottom", "left", "start", "end",
        "gap", "gap-x", "gap-y", "space-x", "space-y",
        "basis",
        "border-spacing", "border-spacing-x", "border-spacing-y",
        "translate", "translate-x", "translate-y", "translate-z",
        "scroll-m", "scroll-mx", "scroll-my", "scroll-ms", "scroll-me",
        "scroll-mbs", "scroll-mbe", "scroll-mt", "scroll-mr", "scroll-mb", "scroll-ml",
        "scroll-p", "scroll-px", "scroll-py", "scroll-ps", "scroll-pe",
        "scroll-pbs", "scroll-pbe", "scroll-pt", "scroll-pr", "scroll-pb", "scroll-pl",
        "indent", "leading"
    };

    // Prefixes that support negative values (prefixed with -)
    private static readonly HashSet<string> NegativePrefixes = new(StringComparer.Ordinal)
    {
        "m", "mx", "my", "mt", "mr", "mb", "ml", "ms", "me", "mbs", "mbe",
        "inset", "inset-x", "inset-y", "inset-s", "inset-e", "inset-bs", "inset-be",
        "top", "right", "bottom", "left", "start", "end",
        "translate", "translate-x", "translate-y", "translate-z",
        "indent", "tracking",
        "scale", "scale-x", "scale-y", "scale-z",
        "rotate", "rotate-x", "rotate-y", "rotate-z",
        "skew", "skew-x", "skew-y",
        "hue-rotate", "backdrop-hue-rotate",
        "order", "col-start", "col-end", "row-start", "row-end",
        "z"
    };

    // Prefixes that take color values
    private static readonly HashSet<string> ColorPrefixes = new(StringComparer.Ordinal)
    {
        "bg", "text", "border", "border-t", "border-r", "border-b", "border-l",
        "border-x", "border-y", "border-s", "border-e", "border-bs", "border-be",
        "ring", "ring-offset", "inset-ring", "outline", "shadow", "inset-shadow",
        "text-shadow", "accent", "caret", "fill", "stroke",
        "divide", "decoration", "placeholder", "from", "via", "to"
    };

    // Prefixes that use specific non-spacing value sets
    private static readonly Dictionary<string, string[]> SpecificValueSets = new()
    {
        ["z"] = new[] { "0", "10", "20", "30", "40", "50", "auto" },
        ["order"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "first", "last", "none" },
        ["col"] = new[] { "auto" },
        ["col-span"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "full" },
        ["col-start"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" },
        ["col-end"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" },
        ["row"] = new[] { "auto" },
        ["row-span"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "full" },
        ["row-start"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" },
        ["row-end"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" },
        ["grid-cols"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "none", "subgrid" },
        ["grid-rows"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "none", "subgrid" },
        ["auto-cols"] = new[] { "auto", "min", "max", "fr" },
        ["auto-rows"] = new[] { "auto", "min", "max", "fr" },
        ["columns"] = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "auto", "3xs", "2xs", "xs", "sm", "md", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl" },
        ["aspect"] = new[] { "auto", "square", "video" },
        ["line-clamp"] = new[] { "1", "2", "3", "4", "5", "6", "none" },
        ["flex"] = new[] { "1", "auto", "initial", "none" },
        ["shrink"] = new[] { "0" },
        ["grow"] = new[] { "0" },
        ["opacity"] = OpacityScale,
        ["backdrop-opacity"] = OpacityScale,
        ["scale"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["scale-x"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["scale-y"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["scale-z"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["rotate"] = new[] { "0", "1", "2", "3", "6", "12", "45", "90", "180" },
        ["rotate-x"] = new[] { "0", "1", "2", "3", "6", "12", "45", "90", "180" },
        ["rotate-y"] = new[] { "0", "1", "2", "3", "6", "12", "45", "90", "180" },
        ["rotate-z"] = new[] { "0", "1", "2", "3", "6", "12", "45", "90", "180" },
        ["skew"] = new[] { "0", "1", "2", "3", "6", "12" },
        ["skew-x"] = new[] { "0", "1", "2", "3", "6", "12" },
        ["skew-y"] = new[] { "0", "1", "2", "3", "6", "12" },
        ["blur"] = new[] { "none", "sm", "md", "lg", "xl", "2xl", "3xl" },
        ["backdrop-blur"] = new[] { "none", "sm", "md", "lg", "xl", "2xl", "3xl" },
        ["brightness"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["backdrop-brightness"] = new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" },
        ["contrast"] = new[] { "0", "50", "75", "100", "125", "150", "200" },
        ["backdrop-contrast"] = new[] { "0", "50", "75", "100", "125", "150", "200" },
        ["grayscale"] = new[] { "0", "100" },
        ["backdrop-grayscale"] = new[] { "0", "100" },
        ["hue-rotate"] = new[] { "0", "15", "30", "60", "90", "180" },
        ["backdrop-hue-rotate"] = new[] { "0", "15", "30", "60", "90", "180" },
        ["invert"] = new[] { "0", "100" },
        ["backdrop-invert"] = new[] { "0", "100" },
        ["saturate"] = new[] { "0", "50", "100", "150", "200" },
        ["backdrop-saturate"] = new[] { "0", "50", "100", "150", "200" },
        ["sepia"] = new[] { "0", "100" },
        ["backdrop-sepia"] = new[] { "0", "100" },
        ["transition"] = new[] { "none", "all", "colors", "opacity", "shadow", "transform" },
        ["duration"] = new[] { "0", "75", "100", "150", "200", "300", "500", "700", "1000", "initial" },
        ["delay"] = new[] { "0", "75", "100", "150", "200", "300", "500", "700", "1000" },
        ["ease"] = new[] { "linear", "in", "out", "in-out" },
        ["animate"] = new[] { "none", "spin", "ping", "pulse", "bounce" },
        ["tracking"] = new[] { "tighter", "tight", "normal", "wide", "wider", "widest" },
        ["font"] = new[] { "sans", "serif", "mono", "thin", "extralight", "light", "normal", "medium", "semibold", "bold", "extrabold", "black" },
        ["text"] = new[] { "xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl", "8xl", "9xl", "left", "center", "right", "justify", "start", "end" },
        ["decoration"] = new[] { "auto", "from-font", "0", "1", "2", "4", "8", "solid", "double", "dotted", "dashed", "wavy" },
        ["underline-offset"] = new[] { "auto", "0", "1", "2", "4", "8" },
        ["outline"] = new[] { "none", "hidden", "solid", "dashed", "dotted", "double" },
        ["outline-offset"] = new[] { "0", "1", "2", "4", "8" },
        ["border"] = new[] { "0", "2", "4", "8" },
        ["border-x"] = new[] { "0", "2", "4", "8" },
        ["border-y"] = new[] { "0", "2", "4", "8" },
        ["border-s"] = new[] { "0", "2", "4", "8" },
        ["border-e"] = new[] { "0", "2", "4", "8" },
        ["border-t"] = new[] { "0", "2", "4", "8" },
        ["border-r"] = new[] { "0", "2", "4", "8" },
        ["border-b"] = new[] { "0", "2", "4", "8" },
        ["border-l"] = new[] { "0", "2", "4", "8" },
        ["divide-x"] = new[] { "0", "2", "4", "8" },
        ["divide-y"] = new[] { "0", "2", "4", "8" },
        ["ring"] = new[] { "0", "1", "2", "4", "8", "inset" },
        ["inset-ring"] = new[] { "0", "1", "2", "4", "8" },
        ["ring-offset"] = new[] { "0", "1", "2", "4", "8" },
        ["shadow"] = new[] { "sm", "md", "lg", "xl", "2xl", "inner", "none", "initial" },
        ["inset-shadow"] = new[] { "sm", "xs", "none", "initial" },
        ["text-shadow"] = new[] { "sm", "md", "lg", "xl", "none", "initial" },
        ["drop-shadow"] = new[] { "sm", "md", "lg", "xl", "2xl", "none" },
        ["rounded"] = new[] { "none", "sm", "md", "lg", "xl", "2xl", "3xl", "full" },
        ["will-change"] = new[] { "auto", "scroll", "contents", "transform" },
        ["content"] = new[] { "none" },
        ["object"] = new[] { "contain", "cover", "fill", "none", "scale-down", "bottom", "center", "left", "left-bottom", "left-top", "right", "right-bottom", "right-top", "top" },
        ["list"] = new[] { "none", "disc", "decimal", "inside", "outside" },
        ["align"] = new[] { "baseline", "top", "middle", "bottom", "text-top", "text-bottom", "sub", "super" },
        ["stroke"] = new[] { "none", "0", "1", "2" },
        ["fill"] = new[] { "none" },
        ["font-stretch"] = new[] { "normal", "ultra-condensed", "extra-condensed", "condensed", "semi-condensed", "semi-expanded", "expanded", "extra-expanded", "ultra-expanded" },
        ["bg-size"] = new[] { "auto", "cover", "contain" },
        ["mask-size"] = new[] { "auto", "cover", "contain" },
        ["perspective"] = new[] { "dramatic", "near", "normal", "midrange", "far", "none" },
        ["contain"] = new[] { "none", "content", "strict", "size", "inline-size", "layout", "paint", "style" },
    };

    // Rounded variants share same value set
    private static readonly string[] RoundedValues = { "none", "sm", "md", "lg", "xl", "2xl", "3xl", "full" };

    public ClassNameAssembler(IReadOnlyList<string> colorFamilies)
    {
        _colorFamilies = colorFamilies;
    }

    public IEnumerable<string> Assemble(DiscoveredUtilities discovered)
    {
        // 1. All static class names directly
        foreach (var name in discovered.StaticClassNames)
            yield return name;

        // 2. Functional prefixes combined with their value scales
        foreach (var prefix in discovered.FunctionalPrefixes)
        {
            // Skip @container and other at-rule utilities
            if (prefix.StartsWith('@'))
                continue;

            // Spacing utilities
            if (SpacingPrefixes.Contains(prefix))
            {
                foreach (var v in SpacingScale)
                    yield return $"{prefix}-{v}";

                // Negative spacing values
                if (NegativePrefixes.Contains(prefix))
                    foreach (var v in SpacingScale)
                        yield return $"-{prefix}-{v}";

                // Fractions for sizing, inset, and translate utilities
                if (SupportsFractions(prefix))
                    foreach (var f in FractionScale)
                        yield return $"{prefix}-{f}";
            }

            // Color values
            if (ColorPrefixes.Contains(prefix))
            {
                foreach (var c in TailwindColorFamilies.SpecialColors)
                    yield return $"{prefix}-{c}";

                foreach (var family in _colorFamilies)
                    foreach (var shade in TailwindColorFamilies.Shades)
                        yield return $"{prefix}-{family}-{shade}";
            }

            // Rounded variants share the same value set
            if (prefix.StartsWith("rounded"))
            {
                foreach (var v in RoundedValues)
                    yield return $"{prefix}-{v}";
                // Also yield the bare prefix (default radius)
                yield return prefix;
                continue;
            }

            // Specific value sets
            if (SpecificValueSets.TryGetValue(prefix, out var values))
            {
                foreach (var v in values)
                    yield return $"{prefix}-{v}";

                // Negative values for appropriate prefixes
                if (NegativePrefixes.Contains(prefix))
                    foreach (var v in values)
                        if (v != "0" && v != "auto" && v != "none" && double.TryParse(v, out _))
                            yield return $"-{prefix}-{v}";
            }

            // Bare prefix (many functional utilities have a default value)
            yield return prefix;
        }
    }

    // Prefixes that accept fractional values (e.g. 1/2, 2/3, 5/12)
    private static readonly HashSet<string> FractionPrefixes = new(StringComparer.Ordinal)
    {
        // Sizing
        "size", "w", "min-w", "max-w", "h", "min-h", "max-h",
        "basis", "inline", "min-inline", "max-inline", "block", "min-block", "max-block",
        // Insets / position
        "inset", "inset-x", "inset-y", "inset-s", "inset-e", "inset-bs", "inset-be",
        "top", "right", "bottom", "left", "start", "end",
        // Translate
        "translate-x", "translate-y",
    };

    private static bool SupportsFractions(string prefix)
    {
        return FractionPrefixes.Contains(prefix);
    }
}
