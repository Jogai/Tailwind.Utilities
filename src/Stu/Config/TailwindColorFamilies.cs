namespace Stu.Config;

public static class TailwindColorFamilies
{
    public static readonly IReadOnlyList<string> AllFamilies = new[]
    {
        "slate", "gray", "zinc", "neutral", "stone",
        "red", "orange", "amber", "yellow", "lime",
        "green", "emerald", "teal", "cyan", "sky",
        "blue", "indigo", "violet", "purple", "fuchsia",
        "pink", "rose"
    };

    public static readonly IReadOnlyList<string> Shades = new[]
    {
        "50", "100", "200", "300", "400", "500",
        "600", "700", "800", "900", "950"
    };

    public static readonly IReadOnlyList<string> SpecialColors = new[]
    {
        "black", "white", "transparent", "current", "inherit"
    };

    /// <summary>
    /// Utility prefixes that take color values (e.g. bg-red-500, text-blue-300).
    /// </summary>
    public static readonly IReadOnlyList<string> ColorUtilityPrefixes = new[]
    {
        "bg", "text", "border", "border-t", "border-r", "border-b", "border-l",
        "border-x", "border-y", "border-s", "border-e",
        "ring", "outline", "shadow", "accent", "caret",
        "fill", "stroke", "divide", "decoration",
        "placeholder", "from", "via", "to"
    };
}
