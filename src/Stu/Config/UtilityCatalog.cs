namespace Stu.Config;

/// <summary>
/// Complete catalog of Tailwind v4 utility class names, organized by category.
/// Used to generate @source inline() directives for the Tailwind CLI.
/// </summary>
public class UtilityCatalog
{
    private readonly IReadOnlyList<string> _colorFamilies;

    public UtilityCatalog(IReadOnlyList<string> colorFamilies)
    {
        _colorFamilies = colorFamilies;
    }

    public IEnumerable<string> GetAllClassNames()
    {
        return Enumerable.Empty<string>()
            .Concat(GetLayoutClasses())
            .Concat(GetSpacingClasses())
            .Concat(GetSizingClasses())
            .Concat(GetTypographyClasses())
            .Concat(GetBackgroundClasses())
            .Concat(GetBorderClasses())
            .Concat(GetEffectClasses())
            .Concat(GetFilterClasses())
            .Concat(GetFlexGridClasses())
            .Concat(GetTransformClasses())
            .Concat(GetTransitionClasses())
            .Concat(GetInteractivityClasses())
            .Concat(GetSvgClasses())
            .Concat(GetAccessibilityClasses())
            .Concat(GetTableClasses())
            .Concat(GetGradientClasses());
    }

    // --- Shared scales ---

    private static readonly string[] SpacingScale =
    {
        "0", "px", "0.5", "1", "1.5", "2", "2.5", "3", "3.5", "4", "5", "6", "7", "8",
        "9", "10", "11", "12", "14", "16", "20", "24", "28", "32", "36", "40", "44",
        "48", "52", "56", "60", "64", "72", "80", "96"
    };

    private static readonly string[] OpacityScale =
    {
        "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
        "55", "60", "65", "70", "75", "80", "85", "90", "95", "100"
    };

    private static readonly string[] FractionScale =
    {
        "1/2", "1/3", "2/3", "1/4", "2/4", "3/4", "1/5", "2/5", "3/5", "4/5",
        "1/6", "2/6", "3/6", "4/6", "5/6", "1/12", "2/12", "3/12", "4/12",
        "5/12", "6/12", "7/12", "8/12", "9/12", "10/12", "11/12"
    };

    // --- Layout ---

    private static IEnumerable<string> GetLayoutClasses()
    {
        // Display
        yield return "hidden";
        yield return "block";
        yield return "inline-block";
        yield return "inline";
        yield return "flex";
        yield return "inline-flex";
        yield return "grid";
        yield return "inline-grid";
        yield return "table";
        yield return "inline-table";
        yield return "table-caption";
        yield return "table-cell";
        yield return "table-column";
        yield return "table-column-group";
        yield return "table-footer-group";
        yield return "table-header-group";
        yield return "table-row-group";
        yield return "table-row";
        yield return "flow-root";
        yield return "contents";
        yield return "list-item";

        // Position
        yield return "static";
        yield return "fixed";
        yield return "absolute";
        yield return "relative";
        yield return "sticky";

        // Inset (top, right, bottom, left)
        foreach (var prefix in new[] { "inset", "inset-x", "inset-y", "top", "right", "bottom", "left", "start", "end" })
        {
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";
            yield return $"-{prefix}-{"{"}all values{"}"}".Length > 0 ? "" : ""; // skip
            foreach (var v in SpacingScale)
                yield return $"-{prefix}-{v}";
            yield return $"{prefix}-auto";
            yield return $"{prefix}-full";
            foreach (var f in FractionScale)
                yield return $"{prefix}-{f}";
        }

        // Visibility
        yield return "visible";
        yield return "invisible";
        yield return "collapse";

        // Float & Clear
        yield return "float-start";
        yield return "float-end";
        yield return "float-right";
        yield return "float-left";
        yield return "float-none";
        yield return "clear-start";
        yield return "clear-end";
        yield return "clear-left";
        yield return "clear-right";
        yield return "clear-both";
        yield return "clear-none";

        // Isolation
        yield return "isolate";
        yield return "isolation-auto";

        // Object Fit
        yield return "object-contain";
        yield return "object-cover";
        yield return "object-fill";
        yield return "object-none";
        yield return "object-scale-down";

        // Object Position
        foreach (var pos in new[] { "bottom", "center", "left", "left-bottom", "left-top", "right", "right-bottom", "right-top", "top" })
            yield return $"object-{pos}";

        // Overflow
        foreach (var axis in new[] { "overflow", "overflow-x", "overflow-y" })
            foreach (var v in new[] { "auto", "hidden", "clip", "visible", "scroll" })
                yield return $"{axis}-{v}";

        // Overscroll
        foreach (var axis in new[] { "overscroll", "overscroll-x", "overscroll-y" })
            foreach (var v in new[] { "auto", "contain", "none" })
                yield return $"{axis}-{v}";

        // Z-Index
        foreach (var v in new[] { "0", "10", "20", "30", "40", "50", "auto" })
            yield return $"z-{v}";

        // Box
        yield return "box-border";
        yield return "box-content";
        yield return "box-decoration-clone";
        yield return "box-decoration-slice";

        // Break
        foreach (var v in new[] { "auto", "avoid", "all", "avoid-page", "page", "left", "right", "column" })
        {
            yield return $"break-before-{v}";
            yield return $"break-after-{v}";
        }
        foreach (var v in new[] { "auto", "avoid", "avoid-page", "avoid-column" })
            yield return $"break-inside-{v}";

        // Columns
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "auto", "3xs", "2xs", "xs", "sm", "md", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl" })
            yield return $"columns-{v}";

        // Aspect Ratio
        yield return "aspect-auto";
        yield return "aspect-square";
        yield return "aspect-video";

        // Container
        yield return "container";
    }

    // --- Spacing ---

    private static IEnumerable<string> GetSpacingClasses()
    {
        var paddingPrefixes = new[] { "p", "px", "py", "pt", "pr", "pb", "pl", "ps", "pe" };
        var marginPrefixes = new[] { "m", "mx", "my", "mt", "mr", "mb", "ml", "ms", "me" };

        foreach (var prefix in paddingPrefixes)
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";

        foreach (var prefix in marginPrefixes)
        {
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";
            // Negative margins
            foreach (var v in SpacingScale.Where(s => s != "0" && s != "px"))
                yield return $"-{prefix}-{v}";
            yield return $"{prefix}-auto";
        }

        // Space between
        foreach (var axis in new[] { "space-x", "space-y" })
        {
            foreach (var v in SpacingScale)
                yield return $"{axis}-{v}";
            yield return $"{axis}-reverse";
        }
    }

    // --- Sizing ---

    private static IEnumerable<string> GetSizingClasses()
    {
        // Width
        foreach (var v in SpacingScale)
            yield return $"w-{v}";
        foreach (var v in new[] { "auto", "full", "screen", "svw", "lvw", "dvw", "min", "max", "fit" })
            yield return $"w-{v}";
        foreach (var f in FractionScale)
            yield return $"w-{f}";

        // Min-width
        foreach (var v in SpacingScale)
            yield return $"min-w-{v}";
        foreach (var v in new[] { "full", "min", "max", "fit" })
            yield return $"min-w-{v}";

        // Max-width
        foreach (var v in SpacingScale)
            yield return $"max-w-{v}";
        foreach (var v in new[] { "none", "full", "min", "max", "fit", "prose" })
            yield return $"max-w-{v}";
        foreach (var v in new[] { "xs", "sm", "md", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl" })
            yield return $"max-w-screen-{v}";

        // Height
        foreach (var v in SpacingScale)
            yield return $"h-{v}";
        foreach (var v in new[] { "auto", "full", "screen", "svh", "lvh", "dvh", "min", "max", "fit" })
            yield return $"h-{v}";
        foreach (var f in FractionScale)
            yield return $"h-{f}";

        // Min-height
        foreach (var v in SpacingScale)
            yield return $"min-h-{v}";
        foreach (var v in new[] { "full", "screen", "svh", "lvh", "dvh", "min", "max", "fit" })
            yield return $"min-h-{v}";

        // Max-height
        foreach (var v in SpacingScale)
            yield return $"max-h-{v}";
        foreach (var v in new[] { "none", "full", "screen", "svh", "lvh", "dvh", "min", "max", "fit" })
            yield return $"max-h-{v}";

        // Size (w + h together)
        foreach (var v in SpacingScale)
            yield return $"size-{v}";
        foreach (var v in new[] { "auto", "full", "min", "max", "fit" })
            yield return $"size-{v}";
        foreach (var f in FractionScale)
            yield return $"size-{f}";
    }

    // --- Typography ---

    private IEnumerable<string> GetTypographyClasses()
    {
        // Font family
        yield return "font-sans";
        yield return "font-serif";
        yield return "font-mono";

        // Font size
        foreach (var v in new[] { "xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl", "8xl", "9xl" })
            yield return $"text-{v}";

        // Font weight
        foreach (var v in new[] { "thin", "extralight", "light", "normal", "medium", "semibold", "bold", "extrabold", "black" })
            yield return $"font-{v}";

        // Font style
        yield return "italic";
        yield return "not-italic";

        // Font smoothing
        yield return "antialiased";
        yield return "subpixel-antialiased";

        // Font variant numeric
        foreach (var v in new[] { "normal-nums", "ordinal", "slashed-zero", "lining-nums", "oldstyle-nums", "proportional-nums", "tabular-nums", "diagonal-fractions", "stacked-fractions" })
            yield return v;

        // Letter spacing
        foreach (var v in new[] { "tighter", "tight", "normal", "wide", "wider", "widest" })
            yield return $"tracking-{v}";

        // Line height
        foreach (var v in new[] { "none", "tight", "snug", "normal", "relaxed", "loose" })
            yield return $"leading-{v}";
        foreach (var v in new[] { "3", "4", "5", "6", "7", "8", "9", "10" })
            yield return $"leading-{v}";

        // Text align
        foreach (var v in new[] { "left", "center", "right", "justify", "start", "end" })
            yield return $"text-{v}";

        // Text color (filtered)
        foreach (var cls in GetColorClasses("text"))
            yield return cls;

        // Text decoration
        yield return "underline";
        yield return "overline";
        yield return "line-through";
        yield return "no-underline";

        // Decoration style
        foreach (var v in new[] { "solid", "double", "dotted", "dashed", "wavy" })
            yield return $"decoration-{v}";

        // Decoration thickness
        foreach (var v in new[] { "auto", "from-font", "0", "1", "2", "4", "8" })
            yield return $"decoration-{v}";

        // Decoration color
        foreach (var cls in GetColorClasses("decoration"))
            yield return cls;

        // Underline offset
        foreach (var v in new[] { "auto", "0", "1", "2", "4", "8" })
            yield return $"underline-offset-{v}";

        // Text transform
        yield return "uppercase";
        yield return "lowercase";
        yield return "capitalize";
        yield return "normal-case";

        // Text overflow
        yield return "truncate";
        yield return "text-ellipsis";
        yield return "text-clip";

        // Text wrap
        yield return "text-wrap";
        yield return "text-nowrap";
        yield return "text-balance";
        yield return "text-pretty";

        // Text indent
        foreach (var v in SpacingScale)
            yield return $"indent-{v}";

        // Vertical align
        foreach (var v in new[] { "baseline", "top", "middle", "bottom", "text-top", "text-bottom", "sub", "super" })
            yield return $"align-{v}";

        // Whitespace
        foreach (var v in new[] { "normal", "nowrap", "pre", "pre-line", "pre-wrap", "break-spaces" })
            yield return $"whitespace-{v}";

        // Word break
        yield return "break-normal";
        yield return "break-words";
        yield return "break-all";
        yield return "break-keep";

        // Hyphens
        yield return "hyphens-none";
        yield return "hyphens-manual";
        yield return "hyphens-auto";

        // Content
        yield return "content-none";

        // List style type
        yield return "list-none";
        yield return "list-disc";
        yield return "list-decimal";

        // List style position
        yield return "list-inside";
        yield return "list-outside";
    }

    // --- Backgrounds ---

    private IEnumerable<string> GetBackgroundClasses()
    {
        // Background color
        foreach (var cls in GetColorClasses("bg"))
            yield return cls;

        // Background attachment
        foreach (var v in new[] { "fixed", "local", "scroll" })
            yield return $"bg-{v}";

        // Background clip
        foreach (var v in new[] { "border", "padding", "content", "text" })
            yield return $"bg-clip-{v}";

        // Background origin
        foreach (var v in new[] { "border", "padding", "content" })
            yield return $"bg-origin-{v}";

        // Background position
        foreach (var v in new[] { "bottom", "center", "left", "left-bottom", "left-top", "right", "right-bottom", "right-top", "top" })
            yield return $"bg-{v}";

        // Background repeat
        foreach (var v in new[] { "repeat", "no-repeat", "repeat-x", "repeat-y", "round", "space" })
            yield return $"bg-{v}";

        // Background size
        foreach (var v in new[] { "auto", "cover", "contain" })
            yield return $"bg-{v}";

        // Background image
        yield return "bg-none";
    }

    // --- Borders ---

    private IEnumerable<string> GetBorderClasses()
    {
        // Border width
        foreach (var prefix in new[] { "border", "border-x", "border-y", "border-t", "border-r", "border-b", "border-l", "border-s", "border-e" })
        {
            yield return prefix; // default width
            foreach (var v in new[] { "0", "2", "4", "8" })
                yield return $"{prefix}-{v}";
        }

        // Border color
        foreach (var prefix in new[] { "border", "border-t", "border-r", "border-b", "border-l", "border-x", "border-y", "border-s", "border-e" })
            foreach (var cls in GetColorClasses(prefix))
                yield return cls;

        // Border style
        foreach (var v in new[] { "solid", "dashed", "dotted", "double", "hidden", "none" })
            yield return $"border-{v}";

        // Border radius
        var radiusSizes = new[] { "none", "sm", "", "md", "lg", "xl", "2xl", "3xl", "full" };
        var radiusPrefixes = new[] { "rounded", "rounded-t", "rounded-r", "rounded-b", "rounded-l", "rounded-tl", "rounded-tr", "rounded-br", "rounded-bl", "rounded-s", "rounded-e", "rounded-ss", "rounded-se", "rounded-es", "rounded-ee" };
        foreach (var prefix in radiusPrefixes)
            foreach (var v in radiusSizes)
                yield return string.IsNullOrEmpty(v) ? prefix : $"{prefix}-{v}";

        // Outline
        foreach (var v in new[] { "none", "", "dashed", "dotted", "double" })
            yield return string.IsNullOrEmpty(v) ? "outline" : $"outline-{v}";
        foreach (var v in new[] { "0", "1", "2", "4", "8" })
            yield return $"outline-{v}";
        foreach (var cls in GetColorClasses("outline"))
            yield return cls;
        foreach (var v in new[] { "0", "1", "2", "4", "8" })
            yield return $"outline-offset-{v}";

        // Ring
        yield return "ring";
        foreach (var v in new[] { "0", "1", "2", "4", "8" })
            yield return $"ring-{v}";
        yield return "ring-inset";
        foreach (var cls in GetColorClasses("ring"))
            yield return cls;

        // Divide
        foreach (var axis in new[] { "divide-x", "divide-y" })
        {
            yield return axis;
            foreach (var v in new[] { "0", "2", "4", "8" })
                yield return $"{axis}-{v}";
            yield return $"{axis}-reverse";
        }
        foreach (var v in new[] { "solid", "dashed", "dotted", "double", "none" })
            yield return $"divide-{v}";
        foreach (var cls in GetColorClasses("divide"))
            yield return cls;
    }

    // --- Effects ---

    private IEnumerable<string> GetEffectClasses()
    {
        // Shadow
        foreach (var v in new[] { "sm", "", "md", "lg", "xl", "2xl", "inner", "none" })
            yield return string.IsNullOrEmpty(v) ? "shadow" : $"shadow-{v}";
        foreach (var cls in GetColorClasses("shadow"))
            yield return cls;

        // Opacity
        foreach (var v in OpacityScale)
            yield return $"opacity-{v}";

        // Mix blend mode
        foreach (var v in new[] { "normal", "multiply", "screen", "overlay", "darken", "lighten", "color-dodge", "color-burn", "hard-light", "soft-light", "difference", "exclusion", "hue", "saturation", "color", "luminosity", "plus-darker", "plus-lighter" })
            yield return $"mix-blend-{v}";

        // Background blend mode
        foreach (var v in new[] { "normal", "multiply", "screen", "overlay", "darken", "lighten", "color-dodge", "color-burn", "hard-light", "soft-light", "difference", "exclusion", "hue", "saturation", "color", "luminosity" })
            yield return $"bg-blend-{v}";
    }

    // --- Filters ---

    private static IEnumerable<string> GetFilterClasses()
    {
        // Blur
        foreach (var v in new[] { "none", "sm", "", "md", "lg", "xl", "2xl", "3xl" })
            yield return string.IsNullOrEmpty(v) ? "blur" : $"blur-{v}";

        // Brightness
        foreach (var v in new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" })
            yield return $"brightness-{v}";

        // Contrast
        foreach (var v in new[] { "0", "50", "75", "100", "125", "150", "200" })
            yield return $"contrast-{v}";

        // Drop shadow
        foreach (var v in new[] { "sm", "", "md", "lg", "xl", "2xl", "none" })
            yield return string.IsNullOrEmpty(v) ? "drop-shadow" : $"drop-shadow-{v}";

        // Grayscale
        yield return "grayscale";
        yield return "grayscale-0";

        // Hue rotate
        foreach (var v in new[] { "0", "15", "30", "60", "90", "180" })
            yield return $"hue-rotate-{v}";

        // Invert
        yield return "invert";
        yield return "invert-0";

        // Saturate
        foreach (var v in new[] { "0", "50", "100", "150", "200" })
            yield return $"saturate-{v}";

        // Sepia
        yield return "sepia";
        yield return "sepia-0";

        // Backdrop filters
        foreach (var v in new[] { "none", "sm", "", "md", "lg", "xl", "2xl", "3xl" })
            yield return string.IsNullOrEmpty(v) ? "backdrop-blur" : $"backdrop-blur-{v}";
        foreach (var v in new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200" })
            yield return $"backdrop-brightness-{v}";
        foreach (var v in new[] { "0", "50", "75", "100", "125", "150", "200" })
            yield return $"backdrop-contrast-{v}";
        yield return "backdrop-grayscale";
        yield return "backdrop-grayscale-0";
        foreach (var v in new[] { "0", "15", "30", "60", "90", "180" })
            yield return $"backdrop-hue-rotate-{v}";
        yield return "backdrop-invert";
        yield return "backdrop-invert-0";
        foreach (var v in OpacityScale)
            yield return $"backdrop-opacity-{v}";
        foreach (var v in new[] { "0", "50", "100", "150", "200" })
            yield return $"backdrop-saturate-{v}";
        yield return "backdrop-sepia";
        yield return "backdrop-sepia-0";
    }

    // --- Flexbox & Grid ---

    private static IEnumerable<string> GetFlexGridClasses()
    {
        // Flex basis
        foreach (var v in SpacingScale)
            yield return $"basis-{v}";
        foreach (var v in new[] { "auto", "full" })
            yield return $"basis-{v}";
        foreach (var f in FractionScale)
            yield return $"basis-{f}";

        // Flex direction
        yield return "flex-row";
        yield return "flex-row-reverse";
        yield return "flex-col";
        yield return "flex-col-reverse";

        // Flex wrap
        yield return "flex-wrap";
        yield return "flex-wrap-reverse";
        yield return "flex-nowrap";

        // Flex
        yield return "flex-1";
        yield return "flex-auto";
        yield return "flex-initial";
        yield return "flex-none";

        // Flex grow/shrink
        yield return "grow";
        yield return "grow-0";
        yield return "shrink";
        yield return "shrink-0";

        // Order
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "first", "last", "none" })
            yield return $"order-{v}";

        // Grid template columns
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "none", "subgrid" })
            yield return $"grid-cols-{v}";

        // Grid column span
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "full" })
            yield return $"col-span-{v}";
        yield return "col-auto";
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" })
        {
            yield return $"col-start-{v}";
            yield return $"col-end-{v}";
        }

        // Grid template rows
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "none", "subgrid" })
            yield return $"grid-rows-{v}";

        // Grid row span
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "full" })
            yield return $"row-span-{v}";
        yield return "row-auto";
        foreach (var v in new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "auto" })
        {
            yield return $"row-start-{v}";
            yield return $"row-end-{v}";
        }

        // Grid auto flow
        yield return "grid-flow-row";
        yield return "grid-flow-col";
        yield return "grid-flow-dense";
        yield return "grid-flow-row-dense";
        yield return "grid-flow-col-dense";

        // Grid auto columns/rows
        foreach (var v in new[] { "auto", "min", "max", "fr" })
        {
            yield return $"auto-cols-{v}";
            yield return $"auto-rows-{v}";
        }

        // Gap
        foreach (var prefix in new[] { "gap", "gap-x", "gap-y" })
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";

        // Justify content
        foreach (var v in new[] { "normal", "start", "end", "center", "between", "around", "evenly", "stretch" })
            yield return $"justify-{v}";

        // Justify items
        foreach (var v in new[] { "start", "end", "center", "stretch" })
            yield return $"justify-items-{v}";

        // Justify self
        foreach (var v in new[] { "auto", "start", "end", "center", "stretch" })
            yield return $"justify-self-{v}";

        // Align content
        foreach (var v in new[] { "normal", "center", "start", "end", "between", "around", "evenly", "baseline", "stretch" })
            yield return $"content-{v}";

        // Align items
        foreach (var v in new[] { "start", "end", "center", "baseline", "stretch" })
            yield return $"items-{v}";

        // Align self
        foreach (var v in new[] { "auto", "start", "end", "center", "baseline", "stretch" })
            yield return $"self-{v}";

        // Place content
        foreach (var v in new[] { "center", "start", "end", "between", "around", "evenly", "baseline", "stretch" })
            yield return $"place-content-{v}";

        // Place items
        foreach (var v in new[] { "start", "end", "center", "baseline", "stretch" })
            yield return $"place-items-{v}";

        // Place self
        foreach (var v in new[] { "auto", "start", "end", "center", "stretch" })
            yield return $"place-self-{v}";
    }

    // --- Transforms ---

    private static IEnumerable<string> GetTransformClasses()
    {
        // Scale
        foreach (var prefix in new[] { "scale", "scale-x", "scale-y" })
            foreach (var v in new[] { "0", "50", "75", "90", "95", "100", "105", "110", "125", "150" })
                yield return $"{prefix}-{v}";

        // Rotate
        foreach (var v in new[] { "0", "1", "2", "3", "6", "12", "45", "90", "180" })
            yield return $"rotate-{v}";
        foreach (var v in new[] { "1", "2", "3", "6", "12", "45", "90", "180" })
            yield return $"-rotate-{v}";

        // Translate
        foreach (var prefix in new[] { "translate-x", "translate-y" })
        {
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";
            foreach (var v in SpacingScale.Where(s => s != "0" && s != "px"))
                yield return $"-{prefix}-{v}";
            yield return $"{prefix}-full";
            foreach (var f in FractionScale)
                yield return $"{prefix}-{f}";
        }

        // Skew
        foreach (var prefix in new[] { "skew-x", "skew-y" })
            foreach (var v in new[] { "0", "1", "2", "3", "6", "12" })
            {
                yield return $"{prefix}-{v}";
                if (v != "0") yield return $"-{prefix}-{v}";
            }

        // Transform origin
        foreach (var v in new[] { "center", "top", "top-right", "right", "bottom-right", "bottom", "bottom-left", "left", "top-left" })
            yield return $"origin-{v}";
    }

    // --- Transitions & Animation ---

    private static IEnumerable<string> GetTransitionClasses()
    {
        // Transition property
        yield return "transition-none";
        yield return "transition-all";
        yield return "transition";
        yield return "transition-colors";
        yield return "transition-opacity";
        yield return "transition-shadow";
        yield return "transition-transform";

        // Duration
        foreach (var v in new[] { "0", "75", "100", "150", "200", "300", "500", "700", "1000" })
            yield return $"duration-{v}";

        // Timing function
        yield return "ease-linear";
        yield return "ease-in";
        yield return "ease-out";
        yield return "ease-in-out";

        // Delay
        foreach (var v in new[] { "0", "75", "100", "150", "200", "300", "500", "700", "1000" })
            yield return $"delay-{v}";

        // Animation
        yield return "animate-none";
        yield return "animate-spin";
        yield return "animate-ping";
        yield return "animate-pulse";
        yield return "animate-bounce";
    }

    // --- Interactivity ---

    private IEnumerable<string> GetInteractivityClasses()
    {
        // Cursor
        foreach (var v in new[] { "auto", "default", "pointer", "wait", "text", "move", "help", "not-allowed", "none", "context-menu", "progress", "cell", "crosshair", "vertical-text", "alias", "copy", "no-drop", "grab", "grabbing", "all-scroll", "col-resize", "row-resize", "n-resize", "e-resize", "s-resize", "w-resize", "ne-resize", "nw-resize", "se-resize", "sw-resize", "ew-resize", "ns-resize", "nesw-resize", "nwse-resize", "zoom-in", "zoom-out" })
            yield return $"cursor-{v}";

        // Pointer events
        yield return "pointer-events-none";
        yield return "pointer-events-auto";

        // Resize
        yield return "resize-none";
        yield return "resize-y";
        yield return "resize-x";
        yield return "resize";

        // Scroll behavior
        yield return "scroll-auto";
        yield return "scroll-smooth";

        // Scroll margin
        foreach (var prefix in new[] { "scroll-m", "scroll-mx", "scroll-my", "scroll-mt", "scroll-mr", "scroll-mb", "scroll-ml", "scroll-ms", "scroll-me" })
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";

        // Scroll padding
        foreach (var prefix in new[] { "scroll-p", "scroll-px", "scroll-py", "scroll-pt", "scroll-pr", "scroll-pb", "scroll-pl", "scroll-ps", "scroll-pe" })
            foreach (var v in SpacingScale)
                yield return $"{prefix}-{v}";

        // Scroll snap align
        yield return "snap-start";
        yield return "snap-end";
        yield return "snap-center";
        yield return "snap-align-none";

        // Scroll snap stop
        yield return "snap-normal";
        yield return "snap-always";

        // Scroll snap type
        yield return "snap-none";
        yield return "snap-x";
        yield return "snap-y";
        yield return "snap-both";
        yield return "snap-mandatory";
        yield return "snap-proximity";

        // Touch action
        foreach (var v in new[] { "auto", "none", "pan-x", "pan-left", "pan-right", "pan-y", "pan-up", "pan-down", "pinch-zoom", "manipulation" })
            yield return $"touch-{v}";

        // User select
        foreach (var v in new[] { "none", "text", "all", "auto" })
            yield return $"select-{v}";

        // Will change
        foreach (var v in new[] { "auto", "scroll", "contents", "transform" })
            yield return $"will-change-{v}";

        // Appearance
        yield return "appearance-none";
        yield return "appearance-auto";

        // Accent color
        yield return "accent-auto";
        foreach (var cls in GetColorClasses("accent"))
            yield return cls;

        // Caret color
        foreach (var cls in GetColorClasses("caret"))
            yield return cls;
    }

    // --- SVG ---

    private IEnumerable<string> GetSvgClasses()
    {
        yield return "fill-none";
        foreach (var cls in GetColorClasses("fill"))
            yield return cls;

        yield return "stroke-none";
        foreach (var cls in GetColorClasses("stroke"))
            yield return cls;
        foreach (var v in new[] { "0", "1", "2" })
            yield return $"stroke-{v}";
    }

    // --- Accessibility ---

    private static IEnumerable<string> GetAccessibilityClasses()
    {
        yield return "sr-only";
        yield return "not-sr-only";
        yield return "forced-color-adjust-auto";
        yield return "forced-color-adjust-none";
    }

    // --- Tables ---

    private static IEnumerable<string> GetTableClasses()
    {
        yield return "border-collapse";
        yield return "border-separate";
        foreach (var v in SpacingScale)
        {
            yield return $"border-spacing-{v}";
            yield return $"border-spacing-x-{v}";
            yield return $"border-spacing-y-{v}";
        }
        yield return "table-auto";
        yield return "table-fixed";
        yield return "caption-top";
        yield return "caption-bottom";
    }

    // --- Gradients ---

    private IEnumerable<string> GetGradientClasses()
    {
        yield return "bg-gradient-to-t";
        yield return "bg-gradient-to-tr";
        yield return "bg-gradient-to-r";
        yield return "bg-gradient-to-br";
        yield return "bg-gradient-to-b";
        yield return "bg-gradient-to-bl";
        yield return "bg-gradient-to-l";
        yield return "bg-gradient-to-tl";

        foreach (var cls in GetColorClasses("from"))
            yield return cls;
        foreach (var cls in GetColorClasses("via"))
            yield return cls;
        foreach (var cls in GetColorClasses("to"))
            yield return cls;
    }

    // --- Color helper ---

    private IEnumerable<string> GetColorClasses(string prefix)
    {
        // Special colors always included
        foreach (var c in TailwindColorFamilies.SpecialColors)
            yield return $"{prefix}-{c}";

        // Family colors filtered by requested families
        foreach (var family in _colorFamilies)
            foreach (var shade in TailwindColorFamilies.Shades)
                yield return $"{prefix}-{family}-{shade}";
    }
}
