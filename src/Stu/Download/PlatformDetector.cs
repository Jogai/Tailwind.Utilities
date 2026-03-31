using System.Runtime.InteropServices;

namespace Stu.Download;

public static class PlatformDetector
{
    public static string GetBinaryName()
    {
        var arch = RuntimeInformation.OSArchitecture;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return arch switch
            {
                Architecture.X64 => "tailwindcss-windows-x64.exe",
                Architecture.Arm64 => "tailwindcss-windows-arm64.exe",
                _ => throw new PlatformNotSupportedException($"Unsupported Windows architecture: {arch}")
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return arch switch
            {
                Architecture.X64 => "tailwindcss-linux-x64",
                Architecture.Arm64 => "tailwindcss-linux-arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported Linux architecture: {arch}")
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return arch switch
            {
                Architecture.X64 => "tailwindcss-macos-x64",
                Architecture.Arm64 => "tailwindcss-macos-arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported macOS architecture: {arch}")
            };
        }

        throw new PlatformNotSupportedException("Unsupported operating system");
    }

    public static bool NeedsExecutablePermission()
    {
        return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
