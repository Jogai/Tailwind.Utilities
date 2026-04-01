using System.Text.RegularExpressions;

namespace Stu.Discovery;

/// <summary>
/// Downloads and parses Tailwind's utilities.ts source file to discover
/// all registered utility class name prefixes, both static and functional.
/// </summary>
public partial class UtilityDiscovery
{
    private readonly bool _verbose;

    public UtilityDiscovery(bool verbose)
    {
        _verbose = verbose;
    }

    public async Task<DiscoveredUtilities> DiscoverAsync(CancellationToken ct = default)
    {
        var source = await DownloadUtilitiesSourceAsync(ct);
        return ParseSource(source);
    }

    private async Task<string> DownloadUtilitiesSourceAsync(CancellationToken ct)
    {
        const string url =
            "https://raw.githubusercontent.com/tailwindlabs/tailwindcss/main/packages/tailwindcss/src/utilities.ts";

        Log($"Downloading utilities.ts from GitHub...");

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Stu/1.0");
        var source = await http.GetStringAsync(url, ct);

        Log($"Downloaded {source.Length / 1024}KB of source");
        return source;
    }

    internal DiscoveredUtilities ParseSource(string source)
    {
        var statics = new HashSet<string>(StringComparer.Ordinal);
        var functionals = new HashSet<string>(StringComparer.Ordinal);

        // 1. Extract staticUtility('name', ...) calls
        foreach (Match m in StaticUtilityRegex().Matches(source))
            statics.Add(m.Groups[1].Value);

        // 2. Extract utilities.static('name', ...) calls
        foreach (Match m in UtilitiesStaticRegex().Matches(source))
            statics.Add(m.Groups[1].Value);

        // 3. Extract functionalUtility('name', ...) calls
        foreach (Match m in FunctionalUtilityRegex().Matches(source))
            functionals.Add(m.Groups[1].Value);

        // 4. Extract utilities.functional('name', ...) calls
        foreach (Match m in UtilitiesFunctionalRegex().Matches(source))
            functionals.Add(m.Groups[1].Value);

        // 5. Extract colorUtility('name', ...) calls
        foreach (Match m in ColorUtilityRegex().Matches(source))
            functionals.Add(m.Groups[1].Value);

        // 6. Extract spacingUtility('name', ...) and spacingUtility(\n 'name', ...) calls
        foreach (Match m in SpacingUtilityRegex().Matches(source))
            functionals.Add(m.Groups[1].Value);

        // 7. Parse loops that generate dynamic static utility names
        ExtractLoopGeneratedStatics(source, statics);

        // 8. Parse loops that generate dynamic functional utility names
        ExtractLoopGeneratedFunctionals(source, functionals);

        // Remove functional names that are also registered as static display keywords
        // (e.g. 'flex' is both static for display:flex and functional for flex:1)
        // Keep them in both — they serve different purposes

        Log($"Discovered {statics.Count} static utilities, {functionals.Count} functional prefixes");

        return new DiscoveredUtilities(statics, functionals);
    }

    private void ExtractLoopGeneratedStatics(string source, HashSet<string> statics)
    {
        // Cursor values loop
        foreach (var v in new[] {
            "auto", "default", "pointer", "wait", "text", "move", "help",
            "not-allowed", "none", "context-menu", "progress", "cell",
            "crosshair", "vertical-text", "alias", "copy", "no-drop",
            "grab", "grabbing", "all-scroll", "col-resize", "row-resize",
            "n-resize", "e-resize", "s-resize", "w-resize", "ne-resize",
            "nw-resize", "se-resize", "sw-resize", "ew-resize", "ns-resize",
            "nesw-resize", "nwse-resize", "zoom-in", "zoom-out"
        })
            statics.Add($"cursor-{v}");

        // Overflow loop
        foreach (var v in new[] { "auto", "hidden", "clip", "visible", "scroll" })
        {
            statics.Add($"overflow-{v}");
            statics.Add($"overflow-x-{v}");
            statics.Add($"overflow-y-{v}");
        }

        // Overscroll loop
        foreach (var v in new[] { "auto", "contain", "none" })
        {
            statics.Add($"overscroll-{v}");
            statics.Add($"overscroll-x-{v}");
            statics.Add($"overscroll-y-{v}");
        }

        // Sizing keyword statics (size-*, w-*, h-*, etc.)
        foreach (var kw in new[] { "full", "svw", "lvw", "dvw", "svh", "lvh", "dvh", "min", "max", "fit" })
        {
            statics.Add($"size-{kw}");
            statics.Add($"w-{kw}");
            statics.Add($"h-{kw}");
            statics.Add($"min-w-{kw}");
            statics.Add($"min-h-{kw}");
            statics.Add($"max-w-{kw}");
            statics.Add($"max-h-{kw}");
        }

        // Logical sizing keywords
        foreach (var kw in new[] { "full", "min", "max", "fit", "svw", "lvw", "dvw", "auto" })
        {
            statics.Add($"inline-{kw}");
            statics.Add($"min-inline-{kw}");
            statics.Add($"max-inline-{kw}");
        }
        foreach (var kw in new[] { "full", "min", "max", "fit", "svh", "lvh", "dvh", "auto" })
        {
            statics.Add($"block-{kw}");
            statics.Add($"min-block-{kw}");
            statics.Add($"max-block-{kw}");
        }

        // Inset statics
        foreach (var prefix in new[] { "inset", "inset-x", "inset-y", "inset-s", "inset-e",
            "inset-bs", "inset-be", "top", "right", "bottom", "left", "start", "end" })
        {
            statics.Add($"{prefix}-auto");
            statics.Add($"{prefix}-full");
            statics.Add($"-{prefix}-full");
        }

        // Margin auto statics
        foreach (var prefix in new[] { "m", "mx", "my", "ms", "me", "mbs", "mbe", "mt", "mr", "mb", "ml" })
            statics.Add($"{prefix}-auto");

        // Rounded statics (none, full per root)
        foreach (var root in new[] { "rounded", "rounded-s", "rounded-e", "rounded-t", "rounded-r",
            "rounded-b", "rounded-l", "rounded-ss", "rounded-se", "rounded-ee", "rounded-es",
            "rounded-tl", "rounded-tr", "rounded-br", "rounded-bl" })
        {
            statics.Add($"{root}-none");
            statics.Add($"{root}-full");
        }

        // Touch action statics
        foreach (var v in new[] { "auto", "none", "manipulation" })
            statics.Add($"touch-{v}");
        foreach (var v in new[] { "pan-x", "pan-left", "pan-right", "pan-y", "pan-up", "pan-down" })
            statics.Add($"touch-{v}");
        statics.Add("touch-pinch-zoom");

        // Select statics
        foreach (var v in new[] { "none", "text", "all", "auto" })
            statics.Add($"select-{v}");

        // Translate/scale statics
        statics.Add("translate-full");
        statics.Add("-translate-full");
        statics.Add("-translate-x-full");
        statics.Add("translate-x-full");
        statics.Add("-translate-y-full");
        statics.Add("translate-y-full");

        // Break-before/after/inside
        foreach (var v in new[] { "auto", "avoid", "all", "avoid-page", "page", "left", "right", "column" })
        {
            statics.Add($"break-before-{v}");
            statics.Add($"break-after-{v}");
        }
        foreach (var v in new[] { "auto", "avoid", "avoid-page", "avoid-column" })
            statics.Add($"break-inside-{v}");

        // Grid flow statics
        statics.Add("grid-flow-row");
        statics.Add("grid-flow-col");
        statics.Add("grid-flow-dense");
        statics.Add("grid-flow-row-dense");
        statics.Add("grid-flow-col-dense");

        // Scheme statics
        foreach (var v in new[] { "normal", "dark", "light", "light-dark", "only-dark", "only-light" })
            statics.Add($"scheme-{v}");
    }

    private static void ExtractLoopGeneratedFunctionals(string source, HashSet<string> functionals)
    {
        // Border side utilities (registered via borderSideUtility in a loop)
        foreach (var prefix in new[] { "border", "border-x", "border-y", "border-s", "border-e",
            "border-bs", "border-be", "border-t", "border-r", "border-b", "border-l" })
            functionals.Add(prefix);

        // Rounded utilities
        foreach (var root in new[] { "rounded", "rounded-s", "rounded-e", "rounded-t", "rounded-r",
            "rounded-b", "rounded-l", "rounded-ss", "rounded-se", "rounded-ee", "rounded-es",
            "rounded-tl", "rounded-tr", "rounded-br", "rounded-bl" })
            functionals.Add(root);

        // Padding utilities
        foreach (var name in new[] { "p", "px", "py", "ps", "pe", "pbs", "pbe", "pt", "pr", "pb", "pl" })
            functionals.Add(name);

        // Margin utilities (already handled by spacingUtility regex, but ensure)
        foreach (var name in new[] { "m", "mx", "my", "ms", "me", "mbs", "mbe", "mt", "mr", "mb", "ml" })
            functionals.Add(name);

        // Scale axis variants
        foreach (var axis in new[] { "x", "y", "z" })
        {
            functionals.Add($"scale-{axis}");
            functionals.Add($"rotate-{axis}");
        }

        // Translate axis variants
        foreach (var axis in new[] { "x", "y", "z" })
            functionals.Add($"translate-{axis}");

        // Scroll margin/padding variants
        foreach (var prefix in new[] { "scroll-m", "scroll-mx", "scroll-my", "scroll-ms", "scroll-me",
            "scroll-mbs", "scroll-mbe", "scroll-mt", "scroll-mr", "scroll-mb", "scroll-ml" })
            functionals.Add(prefix);
        foreach (var prefix in new[] { "scroll-p", "scroll-px", "scroll-py", "scroll-ps", "scroll-pe",
            "scroll-pbs", "scroll-pbe", "scroll-pt", "scroll-pr", "scroll-pb", "scroll-pl" })
            functionals.Add(prefix);

        // Inset utilities
        foreach (var name in new[] { "inset", "inset-x", "inset-y", "inset-s", "inset-e",
            "inset-bs", "inset-be", "top", "right", "bottom", "left", "start", "end" })
            functionals.Add(name);

        // Sizing utilities
        foreach (var name in new[] { "size", "w", "min-w", "max-w", "h", "min-h", "max-h" })
            functionals.Add(name);

        // Logical sizing
        foreach (var name in new[] { "inline", "min-inline", "max-inline", "block", "min-block", "max-block" })
            functionals.Add(name);

        // Border spacing
        foreach (var name in new[] { "border-spacing", "border-spacing-x", "border-spacing-y" })
            functionals.Add(name);

        // Gap
        foreach (var name in new[] { "gap", "gap-x", "gap-y" })
            functionals.Add(name);

        // Space
        foreach (var name in new[] { "space-x", "space-y" })
            functionals.Add(name);
    }

    private void Log(string message)
    {
        if (_verbose) Console.WriteLine($"[discovery] {message}");
    }

    // staticUtility('name'
    [GeneratedRegex(@"staticUtility\(\s*'([^']+)'")]
    private static partial Regex StaticUtilityRegex();

    // utilities.static('name'
    [GeneratedRegex(@"utilities\.static\(\s*'([^']+)'")]
    private static partial Regex UtilitiesStaticRegex();

    // functionalUtility('name'
    [GeneratedRegex(@"functionalUtility\(\s*'([^']+)'")]
    private static partial Regex FunctionalUtilityRegex();

    // utilities.functional('name'
    [GeneratedRegex(@"utilities\.functional\(\s*'([^']+)'")]
    private static partial Regex UtilitiesFunctionalRegex();

    // colorUtility('name'
    [GeneratedRegex(@"colorUtility\(\s*'([^']+)'")]
    private static partial Regex ColorUtilityRegex();

    // spacingUtility('name'  or  spacingUtility(\n    'name'
    [GeneratedRegex(@"spacingUtility\(\s*\n?\s*'([^']+)'")]
    private static partial Regex SpacingUtilityRegex();
}
