using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenVINO.NET.GenAI.Native;

/// <summary>
/// Handles platform-specific native library loading
/// </summary>
internal static class NativeLibraryLoader
{
    private static bool _isInitialized;
    private static readonly object _lock = new();
    private static readonly List<string> _searchPaths = new();
    private static readonly List<string> _loadedLibraries = new();

    /// <summary>
    /// Ensures native libraries are properly configured for the current platform
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SetupWindowsNativeLibraries();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    SetupLinuxNativeLibraries();
                }
                else
                {
                    throw new PlatformNotSupportedException("OpenVINO GenAI is only supported on Windows and Linux");
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new DllNotFoundException(
                    $"Failed to configure OpenVINO GenAI native library loading. " +
                    $"Platform: {RuntimeInformation.OSDescription}, " +
                    $"Architecture: {RuntimeInformation.OSArchitecture}, " +
                    $"Error: {ex.Message}", ex);
            }
        }
    }

    private static void SetupWindowsNativeLibraries()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(assemblyDir))
            throw new InvalidOperationException("Unable to determine assembly directory");

        // Add potential search paths in priority order
        AddSearchPath(assemblyDir); // Assembly directory (main output)
        AddSearchPath(Path.Combine(assemblyDir, "runtimes", "win-x64", "native")); // Standard runtime path
        AddSearchPath(Path.Combine(assemblyDir, "native")); // Alternative native path

        // Set up DLL import resolver for precise control
        NativeLibrary.SetDllImportResolver(typeof(GenAINativeMethods).Assembly, WindowsDllImportResolver);

        // Try to preload critical dependencies
        PreloadWindowsDependencies();
    }

    private static void SetupLinuxNativeLibraries()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(assemblyDir))
            throw new InvalidOperationException("Unable to determine assembly directory");

        // Add potential search paths
        AddSearchPath(assemblyDir);
        AddSearchPath(Path.Combine(assemblyDir, "runtimes", "linux-x64", "native"));
        AddSearchPath(Path.Combine(assemblyDir, "native"));

        // Set up DLL import resolver
        NativeLibrary.SetDllImportResolver(typeof(GenAINativeMethods).Assembly, LinuxDllImportResolver);
    }

    private static void AddSearchPath(string path)
    {
        if (Directory.Exists(path))
        {
            _searchPaths.Add(path);
        }
    }

    private static IntPtr WindowsDllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Handle the main OpenVINO GenAI library
        if (libraryName == "openvino_genai_c")
        {
            return LoadLibraryFromSearchPaths("openvino_genai_c.dll");
        }

        // Let the default resolver handle other libraries
        return IntPtr.Zero;
    }

    private static IntPtr LinuxDllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Handle the main OpenVINO GenAI library
        if (libraryName == "openvino_genai_c")
        {
            // Try different naming conventions for Linux
            var candidates = new[] { "libopenvino_genai_c.so", "openvino_genai_c.so" };

            foreach (var candidate in candidates)
            {
                var handle = LoadLibraryFromSearchPaths(candidate);
                if (handle != IntPtr.Zero)
                    return handle;
            }
        }

        // Let the default resolver handle other libraries
        return IntPtr.Zero;
    }

    private static IntPtr LoadLibraryFromSearchPaths(string libraryName)
    {
        var searchPaths = _searchPaths.ToList();

        // Also try the current directory and standard locations
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            searchPaths.Insert(0, Environment.CurrentDirectory);
        }

        foreach (var searchPath in searchPaths)
        {
            var fullPath = Path.Combine(searchPath, libraryName);
            if (File.Exists(fullPath))
            {
                try
                {
                    if (NativeLibrary.TryLoad(fullPath, out var handle))
                    {
                        _loadedLibraries.Add(fullPath);
                        return handle;
                    }
                }
                catch (Exception ex)
                {
                    // Log the attempt and continue to next path
                    Console.WriteLine($"Failed to load {fullPath}: {ex.Message}");
                }
            }
        }

        // Provide detailed error information
        var availableFiles = new List<string>();
        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                var files = Directory.GetFiles(searchPath, "*.dll", SearchOption.TopDirectoryOnly);
                availableFiles.AddRange(files.Select(f => Path.GetFileName(f)));
            }
        }

        var errorMessage = $"Failed to load native library '{libraryName}'. " +
                          $"Searched paths: {string.Join(", ", searchPaths)}. " +
                          $"Available files: {string.Join(", ", availableFiles.Distinct())}";

        throw new DllNotFoundException(errorMessage);
    }

    private static void PreloadWindowsDependencies()
    {
        // Critical dependencies that need to be loaded first
        var criticalDependencies = new[]
        {
            "openvino_c.dll",
            "openvino.dll",
            "tbb.dll",
            "tbb12.dll"
        };

        foreach (var dependency in criticalDependencies)
        {
            try
            {
                LoadLibraryFromSearchPaths(dependency);
            }
            catch
            {
                // Some dependencies might be optional or named differently
                // Continue loading others
            }
        }
    }

    /// <summary>
    /// Gets diagnostic information about loaded libraries
    /// </summary>
    internal static string GetDiagnosticInfo()
    {
        var info = new List<string>
        {
            $"Platform: {RuntimeInformation.OSDescription}",
            $"Architecture: {RuntimeInformation.OSArchitecture}",
            $"Initialized: {_isInitialized}",
            $"Search paths: {string.Join(", ", _searchPaths)}",
            $"Loaded libraries: {string.Join(", ", _loadedLibraries)}"
        };

        return string.Join(Environment.NewLine, info);
    }
}
