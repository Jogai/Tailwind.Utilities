namespace Stu.Discovery;

/// <summary>
/// Represents utility class names discovered from the Tailwind source.
/// Static utilities are complete class names (e.g. "flex", "hidden").
/// Functional prefixes accept values (e.g. "p" → "p-4", "bg" → "bg-red-500").
/// </summary>
public class DiscoveredUtilities
{
    public IReadOnlySet<string> StaticClassNames { get; }
    public IReadOnlySet<string> FunctionalPrefixes { get; }

    public DiscoveredUtilities(IReadOnlySet<string> statics, IReadOnlySet<string> functionals)
    {
        StaticClassNames = statics;
        FunctionalPrefixes = functionals;
    }
}
