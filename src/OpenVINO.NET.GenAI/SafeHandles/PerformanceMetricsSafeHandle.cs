using System.Runtime.InteropServices;
using OpenVINO.NET.GenAI.Native;

namespace OpenVINO.NET.GenAI.SafeHandles;

/// <summary>
/// Safe handle for Performance Metrics native resources
/// </summary>
public sealed class PerformanceMetricsSafeHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the PerformanceMetricsSafeHandle class
    /// </summary>
    public PerformanceMetricsSafeHandle() : base(IntPtr.Zero, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the PerformanceMetricsSafeHandle class with an existing handle
    /// </summary>
    /// <param name="handle">The existing handle</param>
    /// <param name="ownsHandle">Whether this instance owns the handle</param>
    public PerformanceMetricsSafeHandle(IntPtr handle, bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// Gets a value indicating whether the handle value is invalid
    /// </summary>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Releases the native handle
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            GenAINativeMethods.ov_genai_decoded_results_perf_metrics_free(handle);
            return true;
        }
        return false;
    }
}
