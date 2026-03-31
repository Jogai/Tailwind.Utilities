namespace Stu.Parsing;

public record CssDeclaration(string Property, string Value)
{
    public CssDeclaration WithValue(string newValue) => this with { Value = newValue };
}
