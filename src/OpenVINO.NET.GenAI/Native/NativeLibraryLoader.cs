using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OpenVINO.NET.GenAI.Native;

/// <summary>
/// Handles platform-specific native library loading
/// </summary>
internal static class NativeLibraryLoader
{
    private static bool _isInitialized;
    private static readonly object _lock = new();

    /// <summary>
    /// Ensures native libraries are properly loaded for the current platform
    /// </summary>
    internal static void EnsureLoaded()
    {
        if (_isInitialized)
            return;

        lock (_lock)
        {
            if (_isInitialized)
                return;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    LoadLinuxLibraries();
                }
                // Windows libraries are loaded automatically via DllImport

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new DllNotFoundException(
                    $"Failed to load OpenVINO GenAI native libraries. " +
                    $"Make sure the OpenVINO runtime is installed. " +
                    $"On Linux, you may need to set LD_LIBRARY_PATH or run the setup script. " +
                    $"Error: {ex.Message}", ex);
            }
        }
    }

    private static void LoadLinuxLibraries()
    {
        // Check for environment variable override
        var nativePath = Environment.GetEnvironmentVariable("OPENVINO_GENAI_NATIVE_PATH");
        string[] searchPaths;
        if (!string.IsNullOrEmpty(nativePath))
        {
            Debug.WriteLine($"Using OPENVINO_GENAI_NATIVE_PATH: {nativePath}");
            searchPaths = new[] { nativePath };
        }
        else
        {
            searchPaths = GetLinuxSearchPaths();
        }

        // Core libraries that need to be loaded in order
        var libraries = new[]
        {
            "libtbb.so.12",
            "libopenvino.so.2520",
            "libopenvino_c.so.2520",
            "libopenvino_tokenizers.so",
            "libopenvino_genai.so.2520",
            "libopenvino_genai_c.so.2520"
        };

        foreach (var libName in libraries)
        {
            if (!TryLoadLibrary(libName, searchPaths))
            {
                // Try without version suffix
                var baseLibName = libName.Replace(".2520", "").Replace(".12", "");
                if (!TryLoadLibrary(baseLibName, searchPaths))
                {
                    throw new DllNotFoundException($"Failed to load required library: {libName}");
                }
            }
        }
    }

    private static bool TryLoadLibrary(string libName, string[] searchPaths)
    {
        Debug.WriteLine($"Attempting to load {libName} from system paths or LD_LIBRARY_PATH...");

        if (NativeLibrary.TryLoad(libName, out _))
        {
            Debug.WriteLine($"Successfully loaded {libName} from system paths.");
            return true;
        }

        Debug.WriteLine("Not found in system paths. Trying explicit paths...");

        var triedPaths = new List<string>();

        foreach (var path in searchPaths)
        {
            var fullPath = Path.Combine(path, libName);
            triedPaths.Add(fullPath);

            Debug.WriteLine($"Trying to load: {fullPath}");

            if (File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out _))
            {
                Debug.WriteLine($"Successfully loaded {libName} from {fullPath}");
                return true;
            }
        }

        Debug.WriteLine($"Failed to load {libName}. Tried paths: {string.Join(", ", triedPaths)}");
        return false;
    }

    private static string[] GetLinuxSearchPaths()
    {
        var paths = new List<string>();

        // 1. Output directory (where the app is running from)
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var outputDir = Path.GetDirectoryName(assemblyLocation);
            if (!string.IsNullOrEmpty(outputDir))
            {
                paths.Add(outputDir);

                // Also check runtimes subfolder
                var runtimesPath = Path.Combine(outputDir, "runtimes", "linux-x64", "native");
                if (Directory.Exists(runtimesPath))
                    paths.Add(runtimesPath);
            }
        }

        // 2. Check build directory relative to solution
        var currentDir = Directory.GetCurrentDirectory();
        var buildNativePath = Path.Combine(currentDir, "build", "native", "runtimes", "linux-x64", "native");
        if (Directory.Exists(buildNativePath))
            paths.Add(buildNativePath);

        // 3. Check parent directories for build folder (useful when running from samples)
        var parentDir = currentDir;
        for (int i = 0; i < 4; i++) // Check up to 4 levels up
        {
            parentDir = Path.GetDirectoryName(parentDir);
            if (string.IsNullOrEmpty(parentDir))
                break;

            var parentBuildPath = Path.Combine(parentDir, "build", "native", "runtimes", "linux-x64", "native");
            if (Directory.Exists(parentBuildPath))
                paths.Add(parentBuildPath);
        }

        // 4. Standard system paths
        paths.Add("/usr/local/lib");
        paths.Add("/usr/lib");
        paths.Add("/usr/lib/x86_64-linux-gnu");

        return paths.Distinct().ToArray();
    }
}
