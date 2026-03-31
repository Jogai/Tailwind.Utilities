using Stu.Parsing;

namespace Stu.Transformation;

/// <summary>
/// Merges duplicate selectors, removes duplicate declarations,
/// strips internal --tw-* custom properties, and sorts rules.
/// </summary>
public class CssAggregator
{
    private readonly bool _keepCustomProperties;

    public CssAggregator(bool keepCustomProperties = false)
    {
        _keepCustomProperties = keepCustomProperties;
    }

    public List<CssRule> Aggregate(List<CssRule> rules)
    {
        // Strip --tw-* internal properties unless asked to keep them
        if (!_keepCustomProperties)
        {
            foreach (var rule in rules)
            {
                rule.Declarations = rule.Declarations
                    .Where(d => !d.Property.StartsWith("--tw-"))
                    .ToList();
            }
        }

        // Remove rules that reference unresolved CSS variables as their entire value
        // (these are Tailwind internals that won't work standalone)
        foreach (var rule in rules)
        {
            rule.Declarations = rule.Declarations
                .Where(d => !IsUnresolvedVariable(d.Value))
                .ToList();
        }

        // Remove empty rules
        rules = rules.Where(r => r.Declarations.Count > 0).ToList();

        // Merge rules with the same selector
        var merged = new Dictionary<string, CssRule>(StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            if (merged.TryGetValue(rule.Selector, out var existing))
            {
                // Add declarations that don't already exist
                var existingProps = new HashSet<string>(
                    existing.Declarations.Select(d => d.Property));
                foreach (var decl in rule.Declarations)
                {
                    if (!existingProps.Contains(decl.Property))
                    {
                        existing.Declarations.Add(decl);
                        existingProps.Add(decl.Property);
                    }
                }
            }
            else
            {
                merged[rule.Selector] = new CssRule(rule.Selector, new List<CssDeclaration>(rule.Declarations));
            }
        }

        // Sort by selector for consistent output
        return merged.Values
            .OrderBy(r => r.Selector, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsUnresolvedVariable(string value)
    {
        // Values that are purely var(--tw-...) references with no fallback
        // These won't work in a standalone CSS file
        var trimmed = value.Trim();
        return trimmed.StartsWith("var(--tw-") && trimmed.EndsWith(')') && !trimmed.Contains(',');
    }
}
