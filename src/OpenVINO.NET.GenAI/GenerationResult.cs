using System.Runtime.InteropServices;
using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Result of text generation
/// </summary>
public sealed class GenerationResult : IDisposable
{
    private readonly DecodedResultsSafeHandle _handle;
    private bool _disposed;
    private string? _text;
    private PerformanceMetrics? _performanceMetrics;

    /// <summary>
    /// Internal constructor from existing handle
    /// </summary>
    /// <param name="handle">Existing native handle</param>
    internal GenerationResult(DecodedResultsSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the generated text
    /// </summary>
    public string Text
    {
        get
        {
            if (_text == null)
            {
                _text = GetTextFromNative();
            }
            return _text;
        }
    }

    /// <summary>
    /// Gets the performance metrics for this generation
    /// </summary>
    public PerformanceMetrics PerformanceMetrics
    {
        get
        {
            if (_performanceMetrics == null)
            {
                _performanceMetrics = GetPerformanceMetricsFromNative();
            }
            return _performanceMetrics;
        }
    }

    /// <summary>
    /// Gets the text from the native handle
    /// </summary>
    /// <returns>The generated text</returns>
    private string GetTextFromNative()
    {
        ThrowIfDisposed();

        // First call to get the required buffer size
        nuint bufferSize = 0;
        var status = GenAINativeMethods.ov_genai_decoded_results_get_string(
            _handle.DangerousGetHandle(),
            IntPtr.Zero,
            ref bufferSize);

        OpenVINOGenAIException.ThrowIfError(status, "get string size");

        if (bufferSize == 0)
        {
            return string.Empty;
        }

        // Allocate buffer and get the actual string
        var buffer = Marshal.AllocHGlobal((int)bufferSize);
        try
        {
            status = GenAINativeMethods.ov_genai_decoded_results_get_string(
                _handle.DangerousGetHandle(),
                buffer,
                ref bufferSize);

            OpenVINOGenAIException.ThrowIfError(status, "get string");

            return Marshal.PtrToStringAnsi(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>
    /// Gets the performance metrics from the native handle
    /// </summary>
    /// <returns>The performance metrics</returns>
    private PerformanceMetrics GetPerformanceMetricsFromNative()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_decoded_results_get_perf_metrics(
            _handle.DangerousGetHandle(),
            out var metricsHandle);

        OpenVINOGenAIException.ThrowIfError(status, "get performance metrics");

        return new PerformanceMetrics(new PerformanceMetricsSafeHandle(metricsHandle, true));
    }

    /// <summary>
    /// Returns the generated text
    /// </summary>
    /// <returns>The generated text</returns>
    public override string ToString() => Text;

    /// <summary>
    /// Releases all resources used by the GenerationResult
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _performanceMetrics?.Dispose();
            _handle?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Throws if the object has been disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GenerationResult));
    }
}
