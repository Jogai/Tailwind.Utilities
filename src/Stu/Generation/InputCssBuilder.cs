using System.Text;
using Stu.Config;

namespace Stu.Generation;

public class InputCssBuilder
{
    private readonly UtilityCatalog _catalog;

    public InputCssBuilder(UtilityCatalog catalog)
    {
        _catalog = catalog;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        sb.AppendLine("@import \"tailwindcss\";");
        sb.AppendLine();

        // Disable responsive breakpoints — we only want base utilities
        sb.AppendLine("@theme {");
        sb.AppendLine("  --breakpoint-*: initial;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Collect all class names and group into @source inline() directives.
        // Tailwind v4 uses brace expansion, but for simplicity and reliability
        // we emit one large @source inline() with space-separated class names.
        // Tailwind treats the string as "content" to scan for classes.
        var allClasses = _catalog.GetAllClassNames().Distinct().ToList();

        // Split into chunks to avoid excessively long lines
        const int chunkSize = 200;
        for (var i = 0; i < allClasses.Count; i += chunkSize)
        {
            var chunk = allClasses.Skip(i).Take(chunkSize);
            var classString = string.Join(" ", chunk);
            sb.AppendLine($"@source inline(\"{classString}\");");
        }

        return sb.ToString();
    }

    public string WriteToTempDir()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "stu-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var inputCssPath = Path.Combine(tempDir, "input.css");
        File.WriteAllText(inputCssPath, Build());

        return tempDir;
    }
}
