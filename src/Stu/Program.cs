using System.CommandLine;
using Stu.Config;
using Stu.Download;
using Stu.Generation;
using Stu.Output;
using Stu.Parsing;
using Stu.Transformation;

var outputOption = new Option<string>("-o", ["--output"]) { Description = "Output CSS file path", DefaultValueFactory = _ => "tailwind-utilities.css" };

var colorsOption = new Option<string>("-c", ["--colors"]) { Description = "Comma-separated color families to include, or \"all\"", DefaultValueFactory = _ => "all" };

var cacheDirOption = new Option<string>("--cache-dir")
{
    Description = "Directory to cache the Tailwind binary",
    DefaultValueFactory = _ => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "tailwind")
};

var forceDownloadOption = new Option<bool>("--force-download") { Description = "Force re-download of Tailwind CLI" };

var baseFontSizeOption = new Option<int>("--base-font-size") { Description = "Base font size in px for rem conversion", DefaultValueFactory = _ => 16 };

var minifyOption = new Option<bool>("--minify") { Description = "Output minified CSS" };

var verboseOption = new Option<bool>("-v", ["--verbose"]) { Description = "Verbose logging" };

var rootCommand = new RootCommand("Tailwind CSS Utility Extractor")
{
    outputOption,
    colorsOption,
    cacheDirOption,
    forceDownloadOption,
    baseFontSizeOption,
    minifyOption,
    verboseOption
};

rootCommand.SetAction(async (parseResult, ct) =>
{
    var options = new CliOptions
    {
        Output = parseResult.GetValue(outputOption)!,
        Colors = parseResult.GetValue(colorsOption)!,
        CacheDir = parseResult.GetValue(cacheDirOption)!,
        ForceDownload = parseResult.GetValue(forceDownloadOption),
        BaseFontSize = parseResult.GetValue(baseFontSizeOption),
        Minify = parseResult.GetValue(minifyOption),
        Verbose = parseResult.GetValue(verboseOption)
    };

    try
    {
        await RunPipeline(options, ct);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        if (options.Verbose)
            Console.Error.WriteLine(ex.StackTrace);
        Environment.ExitCode = 1;
    }
});

return await rootCommand.Parse(args).InvokeAsync();

static async Task RunPipeline(CliOptions options, CancellationToken ct)
{
    var verbose = options.Verbose;
    void Log(string phase, string msg)
    {
        if (verbose) Console.WriteLine($"[{phase}] {msg}");
    }

    var colorFamilies = options.GetColorFamilies();
    Log("config", $"Color families: {(options.Colors == "all" ? "all" : string.Join(", ", colorFamilies))}");
    Log("config", $"Base font size: {options.BaseFontSize}px");

    // Step 1: Download Tailwind CLI
    Log("pipeline", "Step 1/6: Downloading Tailwind CLI...");
    var downloader = new TailwindCliDownloader(options.CacheDir, options.ForceDownload, verbose);
    var binaryPath = await downloader.EnsureDownloadedAsync(ct);

    // Step 2: Build input.css
    Log("pipeline", "Step 2/6: Building input.css...");
    var catalog = new UtilityCatalog(colorFamilies);
    var builder = new InputCssBuilder(catalog);
    var workDir = builder.WriteToTempDir();
    Log("generation", $"Work directory: {workDir}");

    try
    {
        // Step 3: Run Tailwind CLI
        Log("pipeline", "Step 3/6: Running Tailwind CLI...");
        var runner = new TailwindRunner(binaryPath, verbose);
        var outputCssPath = await runner.RunAsync(workDir, ct);

        // Step 4: Parse CSS
        Log("pipeline", "Step 4/6: Parsing generated CSS...");
        var rawCss = await File.ReadAllTextAsync(outputCssPath, ct);
        var parser = new CssParser();
        var rules = parser.Parse(rawCss);
        Log("parsing", $"Parsed {rules.Count} rules");

        // Step 5: Filter & Transform
        Log("pipeline", "Step 5/6: Filtering and transforming...");

        rules = VariantFilter.Filter(rules);
        Log("filter", $"After variant filter: {rules.Count} rules");

        rules = ArbitraryValueFilter.Filter(rules);
        Log("filter", $"After arbitrary value filter: {rules.Count} rules");

        // Transform values
        var unitConverter = new UnitConverter(options.BaseFontSize);
        var colorConverter = new ColorConverter();

        foreach (var rule in rules)
        {
            for (var i = 0; i < rule.Declarations.Count; i++)
            {
                var decl = rule.Declarations[i];
                var value = decl.Value;
                value = unitConverter.Convert(value);
                value = colorConverter.Convert(value);
                rule.Declarations[i] = decl.WithValue(value);
            }
        }

        // Aggregate
        var aggregator = new CssAggregator();
        rules = aggregator.Aggregate(rules);
        Log("aggregate", $"After aggregation: {rules.Count} rules");

        // Step 6: Write output
        Log("pipeline", "Step 6/6: Writing output...");
        var writer = new CssWriter(options.Minify);
        writer.WriteToFile(rules, options.Output);

        var fileSize = new FileInfo(options.Output).Length;
        Console.WriteLine($"Written {rules.Count} rules to {options.Output} ({fileSize / 1024}KB)");
    }
    finally
    {
        // Cleanup temp directory
        try
        {
            Directory.Delete(workDir, recursive: true);
            Log("cleanup", $"Deleted temp directory: {workDir}");
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
