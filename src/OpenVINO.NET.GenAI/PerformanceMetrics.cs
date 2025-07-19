using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Performance metrics for text generation
/// </summary>
public sealed class PerformanceMetrics : IDisposable
{
    private readonly PerformanceMetricsSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Internal constructor from existing handle
    /// </summary>
    /// <param name="handle">Existing native handle</param>
    internal PerformanceMetrics(PerformanceMetricsSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the load time in milliseconds
    /// </summary>
    public float LoadTime
    {
        get
        {
            ThrowIfDisposed();
            var status = GenAINativeMethods.ov_genai_perf_metrics_get_load_time(_handle.DangerousGetHandle(), out var loadTime);
            OpenVINOGenAIException.ThrowIfError(status, "get load time");
            return loadTime;
        }
    }

    /// <summary>
    /// Gets the number of generated tokens
    /// </summary>
    public int NumGenerationTokens
    {
        get
        {
            ThrowIfDisposed();
            var status = GenAINativeMethods.ov_genai_perf_metrics_get_num_generation_tokens(_handle.DangerousGetHandle(), out var numTokens);
            OpenVINOGenAIException.ThrowIfError(status, "get number of generation tokens");
            return (int)numTokens;
        }
    }

    /// <summary>
    /// Gets the number of input tokens
    /// </summary>
    public int NumInputTokens
    {
        get
        {
            ThrowIfDisposed();
            var status = GenAINativeMethods.ov_genai_perf_metrics_get_num_input_tokens(_handle.DangerousGetHandle(), out var numTokens);
            OpenVINOGenAIException.ThrowIfError(status, "get number of input tokens");
            return (int)numTokens;
        }
    }

    /// <summary>
    /// Gets the time to first token (TTFT) statistics
    /// </summary>
    /// <returns>A tuple containing (mean, standard deviation) in milliseconds</returns>
    public (float Mean, float Std) GetTimeToFirstToken()
    {
        ThrowIfDisposed();
        var status = GenAINativeMethods.ov_genai_perf_metrics_get_ttft(_handle.DangerousGetHandle(), out var mean, out var std);
        OpenVINOGenAIException.ThrowIfError(status, "get time to first token");
        return (mean, std);
    }

    /// <summary>
    /// Gets the time per output token (TPOT) statistics
    /// </summary>
    /// <returns>A tuple containing (mean, standard deviation) in milliseconds</returns>
    public (float Mean, float Std) GetTimePerOutputToken()
    {
        ThrowIfDisposed();
        var status = GenAINativeMethods.ov_genai_perf_metrics_get_tpot(_handle.DangerousGetHandle(), out var mean, out var std);
        OpenVINOGenAIException.ThrowIfError(status, "get time per output token");
        return (mean, std);
    }

    /// <summary>
    /// Gets the throughput statistics (tokens per second)
    /// </summary>
    /// <returns>A tuple containing (mean, standard deviation) in tokens per second</returns>
    public (float Mean, float Std) GetThroughput()
    {
        ThrowIfDisposed();
        var status = GenAINativeMethods.ov_genai_perf_metrics_get_throughput(_handle.DangerousGetHandle(), out var mean, out var std);
        OpenVINOGenAIException.ThrowIfError(status, "get throughput");
        return (mean, std);
    }

    /// <summary>
    /// Gets the average tokens per second
    /// </summary>
    public float TokensPerSecond => GetThroughput().Mean;

    /// <summary>
    /// Gets the mean time to first token in milliseconds
    /// </summary>
    public float FirstTokenLatency => GetTimeToFirstToken().Mean;

    /// <summary>
    /// Releases all resources used by the PerformanceMetrics
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
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
            throw new ObjectDisposedException(nameof(PerformanceMetrics));
    }
}
