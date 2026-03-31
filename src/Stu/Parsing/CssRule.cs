namespace Stu.Parsing;

public class CssRule
{
    public string Selector { get; set; } = "";
    public List<CssDeclaration> Declarations { get; set; } = new();

    public CssRule() { }

    public CssRule(string selector, List<CssDeclaration> declarations)
    {
        Selector = selector;
        Declarations = declarations;
    }
}
