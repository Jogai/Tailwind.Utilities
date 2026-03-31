using System.Diagnostics;

namespace Stu.Download;

public class TailwindCliDownloader
{
    private const string DownloadUrlTemplate =
        "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/{0}";

    private readonly string _cacheDir;
    private readonly bool _forceDownload;
    private readonly bool _verbose;

    public TailwindCliDownloader(string cacheDir, bool forceDownload, bool verbose)
    {
        _cacheDir = cacheDir;
        _forceDownload = forceDownload;
        _verbose = verbose;
    }

    public async Task<string> EnsureDownloadedAsync(CancellationToken ct = default)
    {
        var binaryName = PlatformDetector.GetBinaryName();
        var binaryPath = Path.Combine(_cacheDir, binaryName);

        if (!_forceDownload && File.Exists(binaryPath))
        {
            Log($"Using cached Tailwind CLI: {binaryPath}");
            return binaryPath;
        }

        Directory.CreateDirectory(_cacheDir);

        var url = string.Format(DownloadUrlTemplate, binaryName);
        Log($"Downloading Tailwind CLI from {url}...");

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Stu/1.0");

        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(binaryPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[81920];
        long downloaded = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            downloaded += bytesRead;

            if (_verbose && totalBytes.HasValue)
            {
                var pct = (double)downloaded / totalBytes.Value * 100;
                Console.Write($"\r  Progress: {pct:F0}% ({downloaded / 1024 / 1024}MB)");
            }
        }

        if (_verbose) Console.WriteLine();

        Log($"Downloaded to {binaryPath} ({downloaded / 1024 / 1024}MB)");

        if (PlatformDetector.NeedsExecutablePermission())
        {
            var chmod = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                ArgumentList = { "+x", binaryPath },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            if (chmod != null)
                await chmod.WaitForExitAsync(ct);
        }

        return binaryPath;
    }

    private void Log(string message)
    {
        if (_verbose)
            Console.WriteLine($"[download] {message}");
    }
}
