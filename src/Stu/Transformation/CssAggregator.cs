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

        // Remove declarations with empty/whitespace-only values
        foreach (var rule in rules)
        {
            rule.Declarations = rule.Declarations
                .Where(d => !string.IsNullOrWhiteSpace(d.Value))
                .ToList();
        }

        // Remove declarations that reference unresolved --tw-* CSS variables
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
        // Any value that still references --tw-* variables won't work standalone.
        // This covers both simple `var(--tw-shadow)` and composite values like
        // `var(--tw-inset-shadow), var(--tw-ring-shadow), var(--tw-shadow)`.
        return value.Contains("var(--tw-");
    }
}
