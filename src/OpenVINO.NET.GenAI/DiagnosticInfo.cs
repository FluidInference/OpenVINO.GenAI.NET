using Fluid.OpenVINO.GenAI.Native;

namespace Fluid.OpenVINO.GenAI;

/// <summary>
/// Provides diagnostic information about OpenVINO GenAI library loading
/// </summary>
public static class DiagnosticInfo
{
    /// <summary>
    /// Gets comprehensive diagnostic information about native library loading
    /// </summary>
    /// <returns>A string containing diagnostic information</returns>
    public static string GetNativeLibraryDiagnostics()
    {
        return NativeLibraryLoader.GetDiagnosticInfo();
    }
}