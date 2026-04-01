using System.Text;

namespace Stu.Generation;

public class InputCssBuilder
{
    private readonly IEnumerable<string> _classNames;

    public InputCssBuilder(IEnumerable<string> classNames)
    {
        _classNames = classNames;
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
        // Tailwind treats the string as "content" to scan for classes.
        var allClasses = _classNames.Distinct().ToList();

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
