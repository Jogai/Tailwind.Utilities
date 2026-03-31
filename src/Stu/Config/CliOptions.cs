namespace Stu.Config;

public class CliOptions
{
    public string Output { get; set; } = "tailwind-utilities.css";
    public string Colors { get; set; } = "all";
    public string CacheDir { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".stu", "cache");
    public bool ForceDownload { get; set; }
    public int BaseFontSize { get; set; } = 16;
    public bool Minify { get; set; }
    public bool Verbose { get; set; }

    public IReadOnlyList<string> GetColorFamilies()
    {
        if (string.Equals(Colors, "all", StringComparison.OrdinalIgnoreCase))
            return TailwindColorFamilies.AllFamilies;

        return Colors
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.ToLowerInvariant())
            .Where(c => TailwindColorFamilies.AllFamilies.Contains(c))
            .ToList();
    }
}
