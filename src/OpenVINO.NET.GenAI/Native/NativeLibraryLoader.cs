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
                    Console.WriteLine("Setting up Windows native libraries");
                    SetupWindowsNativeLibraries();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Setting up Linux native libraries");
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

        // First priority: Check OPENVINO_RUNTIME_PATH environment variable
        var envPath = Environment.GetEnvironmentVariable("OPENVINO_RUNTIME_PATH");
        Console.WriteLine($"OPENVINO_RUNTIME_PATH: {envPath}");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            Console.WriteLine($"Adding search paths from OPENVINO_RUNTIME_PATH: {envPath}");
            AddSearchPathsRecursively(envPath);
        }

        // Add potential search paths in priority order
        AddSearchPath(assemblyDir); // Assembly directory (main output)
        AddSearchPathsRecursively(assemblyDir); // Search subdirectories for nested structures
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

        // First priority: Check OPENVINO_RUNTIME_PATH environment variable
        var envPath = Environment.GetEnvironmentVariable("OPENVINO_RUNTIME_PATH");
        Console.WriteLine($"OPENVINO_RUNTIME_PATH: {envPath}");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            Console.WriteLine($"Adding search paths from OPENVINO_RUNTIME_PATH: {envPath}");
            AddSearchPathsRecursively(envPath);
        }

        // Add potential search paths
        AddSearchPath(assemblyDir);
        AddSearchPathsRecursively(assemblyDir); // Search subdirectories for nested structures
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

    private static void AddSearchPathsRecursively(string basePath)
    {
        if (!Directory.Exists(basePath))
            return;

        AddSearchPath(basePath);

        try
        {
            // Add subdirectories that might contain DLLs
            var subdirs = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories);
            foreach (var subdir in subdirs)
            {
                // Only add directories that actually contain DLL/SO files
                var nativeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "*.dll" : "*.so";
                if (Directory.GetFiles(subdir, nativeExtension).Length > 0)
                {
                    AddSearchPath(subdir);
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - recursive search is best-effort
            Console.WriteLine($"Warning: Could not search subdirectories of {basePath}: {ex.Message}");
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
                    var handle = NativeLibrary.Load(fullPath);
                    _loadedLibraries.Add(fullPath);
                    Console.WriteLine($"Successfully loaded: {fullPath}");
                    return handle;
                }
                catch (Exception ex)
                {
                    // Log the detailed attempt and continue to next path
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
            "openvino_cd.dll",
            "openvino_genai_c.dll",
            "openvino_genai_cd.dll",
            "openvino.dll", 
            "openvinod.dll",
            "tbb.dll",
            "tbb12.dll"
        };

        foreach (var dependency in criticalDependencies)
        {
            try
            {
                LoadLibraryFromSearchPaths(dependency);
            }
            catch (Exception ex)
            {
                // Log dependency loading attempts - these might be optional
                Console.WriteLine($"Could not preload dependency {dependency}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets diagnostic information about loaded libraries
    /// </summary>
    internal static string GetDiagnosticInfo()
    {
        var envPath = Environment.GetEnvironmentVariable("OPENVINO_RUNTIME_PATH");

        var info = new List<string>
        {
            $"Platform: {RuntimeInformation.OSDescription}",
            $"Architecture: {RuntimeInformation.OSArchitecture}",
            $"Initialized: {_isInitialized}",
            $"OPENVINO_RUNTIME_PATH: {envPath ?? "(not set)"}",
            $"Search paths: {string.Join(", ", _searchPaths)}",
            $"Loaded libraries: {string.Join(", ", _loadedLibraries)}"
        };

        return string.Join(Environment.NewLine, info);
    }
}
