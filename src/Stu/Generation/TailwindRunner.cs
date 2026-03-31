using System.Diagnostics;

namespace Stu.Generation;

public class TailwindRunner
{
    private readonly string _binaryPath;
    private readonly bool _verbose;

    public TailwindRunner(string binaryPath, bool verbose)
    {
        _binaryPath = binaryPath;
        _verbose = verbose;
    }

    public async Task<string> RunAsync(string workDir, CancellationToken ct = default)
    {
        var inputPath = Path.Combine(workDir, "input.css");
        var outputPath = Path.Combine(workDir, "output.css");

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("input.css not found in work directory", inputPath);

        Log($"Running Tailwind CLI: {_binaryPath} -i input.css -o output.css");

        var psi = new ProcessStartInfo
        {
            FileName = _binaryPath,
            ArgumentList = { "-i", inputPath, "-o", outputPath },
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };

        var stdout = new List<string>();
        var stderr = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) stdout.Add(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) stderr.Add(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

        try
        {
            await process.WaitForExitAsync(linked.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("Tailwind CLI timed out after 3 minutes");
        }

        if (_verbose)
        {
            foreach (var line in stdout) Console.WriteLine($"  [tw stdout] {line}");
            foreach (var line in stderr) Console.WriteLine($"  [tw stderr] {line}");
        }

        if (process.ExitCode != 0)
        {
            var errorOutput = string.Join(Environment.NewLine, stderr);
            throw new InvalidOperationException(
                $"Tailwind CLI exited with code {process.ExitCode}:\n{errorOutput}");
        }

        if (!File.Exists(outputPath))
            throw new FileNotFoundException("Tailwind CLI did not produce output.css", outputPath);

        var outputSize = new FileInfo(outputPath).Length;
        Log($"Tailwind generated {outputSize / 1024}KB of CSS");

        return outputPath;
    }

    private void Log(string message)
    {
        if (_verbose)
            Console.WriteLine($"[tailwind] {message}");
    }
}
