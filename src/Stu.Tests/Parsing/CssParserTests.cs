using Stu.Parsing;
using Xunit;

namespace Stu.Tests.Parsing;

public class CssParserTests
{
    private readonly CssParser _parser = new();

    [Fact]
    public void ParseSimpleRule()
    {
        var css = ".text-red-500 { color: red; }";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(".text-red-500", rules[0].Selector);
        Assert.Single(rules[0].Declarations);
        Assert.Equal("color", rules[0].Declarations[0].Property);
        Assert.Equal("red", rules[0].Declarations[0].Value);
    }

    [Fact]
    public void ParseMultipleDeclarations()
    {
        var css = ".p-4 { padding-top: 1rem; padding-right: 1rem; padding-bottom: 1rem; padding-left: 1rem; }";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(4, rules[0].Declarations.Count);
    }

    [Fact]
    public void ParseMultipleRules()
    {
        var css = @"
            .block { display: block; }
            .flex { display: flex; }
            .hidden { display: none; }
        ";
        var rules = _parser.Parse(css);

        Assert.Equal(3, rules.Count);
    }

    [Fact]
    public void UnwrapLayerBlocks()
    {
        var css = @"
            @layer utilities {
                .block { display: block; }
                .flex { display: flex; }
            }
        ";
        var rules = _parser.Parse(css);

        Assert.Equal(2, rules.Count);
        Assert.Equal(".block", rules[0].Selector);
    }

    [Fact]
    public void StripKeyframes()
    {
        var css = @"
            @keyframes spin {
                from { transform: rotate(0deg); }
                to { transform: rotate(360deg); }
            }
            .animate-spin { animation: spin 1s linear infinite; }
        ";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(".animate-spin", rules[0].Selector);
    }

    [Fact]
    public void StripMediaQueries()
    {
        var css = @"
            .block { display: block; }
            @media (min-width: 640px) {
                .sm\:block { display: block; }
            }
        ";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(".block", rules[0].Selector);
    }

    [Fact]
    public void StripPropertyBlocks()
    {
        var css = @"
            @property --tw-shadow {
                syntax: '*';
                inherits: false;
            }
            .shadow { box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        ";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(".shadow", rules[0].Selector);
    }

    [Fact]
    public void HandleColorFunctionInValue()
    {
        var css = ".bg-red-500 { background-color: oklch(0.637 0.237 25.331); }";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal("oklch(0.637 0.237 25.331)", rules[0].Declarations[0].Value);
    }

    [Fact]
    public void SkipAtRulesInSelectors()
    {
        var css = @"
            @charset ""UTF-8"";
            .block { display: block; }
        ";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(".block", rules[0].Selector);
    }

    [Fact]
    public void ParseEscapedSelectors()
    {
        var css = @".w-1\/2 { width: 50%; }";
        var rules = _parser.Parse(css);

        Assert.Single(rules);
        Assert.Equal(@".w-1\/2", rules[0].Selector);
    }
}
