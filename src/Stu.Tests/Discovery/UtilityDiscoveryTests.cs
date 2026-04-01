using Stu.Discovery;
using Xunit;

namespace Stu.Tests.Discovery;

public class UtilityDiscoveryTests
{
    private readonly UtilityDiscovery _discovery = new(verbose: false);

    [Fact]
    public void ParsesStaticUtilities()
    {
        var source = """
            staticUtility('flex', [['display', 'flex']])
            staticUtility('hidden', [['display', 'none']])
            staticUtility('sr-only', [['position', 'absolute']])
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("flex", result.StaticClassNames);
        Assert.Contains("hidden", result.StaticClassNames);
        Assert.Contains("sr-only", result.StaticClassNames);
    }

    [Fact]
    public void ParsesUtilitiesStaticCalls()
    {
        var source = """
            utilities.static('outline-hidden', () => { })
            utilities.static('container', () => { })
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("outline-hidden", result.StaticClassNames);
        Assert.Contains("container", result.StaticClassNames);
    }

    [Fact]
    public void ParsesFunctionalUtilities()
    {
        var source = """
            functionalUtility('z', { themeKeys: ['--z-index'] })
            functionalUtility('opacity', { themeKeys: ['--opacity'] })
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("z", result.FunctionalPrefixes);
        Assert.Contains("opacity", result.FunctionalPrefixes);
    }

    [Fact]
    public void ParsesUtilitiesFunctionalCalls()
    {
        var source = """
            utilities.functional('text', (candidate) => { })
            utilities.functional('shadow', (candidate) => { })
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("text", result.FunctionalPrefixes);
        Assert.Contains("shadow", result.FunctionalPrefixes);
    }

    [Fact]
    public void ParsesColorUtilities()
    {
        var source = """
            colorUtility('accent', { themeKeys: ['--accent-color'] })
            colorUtility('caret', { themeKeys: ['--caret-color'] })
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("accent", result.FunctionalPrefixes);
        Assert.Contains("caret", result.FunctionalPrefixes);
    }

    [Fact]
    public void ParsesSpacingUtilities()
    {
        var source = """
            spacingUtility('gap', ['--gap', '--spacing'], (value) => [decl('gap', value)])
            spacingUtility(
                'leading',
                ['--leading', '--spacing'],
                (value) => [decl('line-height', value)]
            )
            """;

        var result = _discovery.ParseSource(source);

        Assert.Contains("gap", result.FunctionalPrefixes);
        Assert.Contains("leading", result.FunctionalPrefixes);
    }

    [Fact]
    public void IncludesLoopGeneratedPadding()
    {
        // Padding utilities are generated in a loop — our hardcoded extraction should find them
        var source = ""; // empty source, loop-generated statics are always added
        var result = _discovery.ParseSource(source);

        Assert.Contains("p", result.FunctionalPrefixes);
        Assert.Contains("px", result.FunctionalPrefixes);
        Assert.Contains("pt", result.FunctionalPrefixes);
        Assert.Contains("pb", result.FunctionalPrefixes);
    }

    [Fact]
    public void IncludesLoopGeneratedRounded()
    {
        var source = "";
        var result = _discovery.ParseSource(source);

        Assert.Contains("rounded", result.FunctionalPrefixes);
        Assert.Contains("rounded-tl", result.FunctionalPrefixes);
        Assert.Contains("rounded-br", result.FunctionalPrefixes);
    }

    [Fact]
    public void IncludesLoopGeneratedBorder()
    {
        var source = "";
        var result = _discovery.ParseSource(source);

        Assert.Contains("border", result.FunctionalPrefixes);
        Assert.Contains("border-t", result.FunctionalPrefixes);
        Assert.Contains("border-x", result.FunctionalPrefixes);
    }

    [Fact]
    public void IncludesOverflowStatics()
    {
        var source = "";
        var result = _discovery.ParseSource(source);

        Assert.Contains("overflow-hidden", result.StaticClassNames);
        Assert.Contains("overflow-x-auto", result.StaticClassNames);
        Assert.Contains("overflow-y-scroll", result.StaticClassNames);
    }

    [Fact]
    public void IncludesCursorStatics()
    {
        var source = "";
        var result = _discovery.ParseSource(source);

        Assert.Contains("cursor-pointer", result.StaticClassNames);
        Assert.Contains("cursor-not-allowed", result.StaticClassNames);
    }
}
