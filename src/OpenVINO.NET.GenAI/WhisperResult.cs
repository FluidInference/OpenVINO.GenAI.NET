using System.Runtime.InteropServices;
using System.Text;
using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Represents a transcribed text chunk with timing information
/// </summary>
public sealed class WhisperChunk
{
    /// <summary>
    /// Gets the start time of the chunk in seconds
    /// </summary>
    public float StartTime { get; }

    /// <summary>
    /// Gets the end time of the chunk in seconds
    /// </summary>
    public float EndTime { get; }

    /// <summary>
    /// Gets the transcribed text
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the duration of the chunk in seconds
    /// </summary>
    public float Duration => EndTime - StartTime;

    /// <summary>
    /// Initializes a new instance of the WhisperChunk class
    /// </summary>
    /// <param name="startTime">Start time in seconds</param>
    /// <param name="endTime">End time in seconds</param>
    /// <param name="text">Transcribed text</param>
    internal WhisperChunk(float startTime, float endTime, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Text = text ?? string.Empty;
    }

    /// <summary>
    /// Returns a string representation of the chunk
    /// </summary>
    /// <returns>String representation with timing and text</returns>
    public override string ToString()
    {
        return $"[{StartTime:F2}s - {EndTime:F2}s]: {Text}";
    }
}

/// <summary>
/// Represents the result of Whisper speech transcription
/// </summary>
public sealed class WhisperResult : IDisposable
{
    private readonly WhisperResultsSafeHandle _handle;
    private bool _disposed;
    private string[]? _texts;
    private float[]? _scores;
    private WhisperChunk[]? _chunks;
    private WhisperPerformanceMetrics? _performanceMetrics;

    /// <summary>
    /// Initializes a new instance of the WhisperResult class
    /// </summary>
    /// <param name="handle">The native handle for the results</param>
    internal WhisperResult(WhisperResultsSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the transcribed texts
    /// </summary>
    public string[] Texts
    {
        get
        {
            ThrowIfDisposed();
            if (_texts == null)
            {
                _texts = LoadTexts();
            }
            return _texts;
        }
    }

    /// <summary>
    /// Gets the confidence scores for each text
    /// </summary>
    public float[] Scores
    {
        get
        {
            ThrowIfDisposed();
            if (_scores == null)
            {
                _scores = LoadScores();
            }
            return _scores;
        }
    }

    /// <summary>
    /// Gets the timestamped chunks (if timestamps were requested)
    /// </summary>
    public WhisperChunk[] Chunks
    {
        get
        {
            ThrowIfDisposed();
            if (_chunks == null)
            {
                _chunks = LoadChunks();
            }
            return _chunks;
        }
    }

    /// <summary>
    /// Gets the performance metrics for the transcription
    /// </summary>
    public WhisperPerformanceMetrics PerformanceMetrics
    {
        get
        {
            ThrowIfDisposed();
            if (_performanceMetrics == null)
            {
                _performanceMetrics = LoadPerformanceMetrics();
            }
            return _performanceMetrics;
        }
    }

    /// <summary>
    /// Gets the main transcribed text (first text if multiple)
    /// </summary>
    public string Text => Texts.Length > 0 ? Texts[0] : string.Empty;

    /// <summary>
    /// Gets the full transcribed text with timestamps (if available)
    /// </summary>
    public string FullText => Chunks.Length > 0 ? string.Join("\n", Chunks.AsEnumerable()) : Text;

    /// <summary>
    /// Loads the transcribed texts from the native handle
    /// </summary>
    private string[] LoadTexts()
    {
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_texts_size(_handle.DangerousGetHandle(), out var textsSize);
        OpenVINOGenAIException.ThrowIfError(status, "get texts size");

        var texts = new string[textsSize];
        for (nuint i = 0; i < textsSize; i++)
        {
            // Get the required buffer size
            nuint bufferSize = 0;
            status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_text_at(_handle.DangerousGetHandle(), i, IntPtr.Zero, ref bufferSize);
            OpenVINOGenAIException.ThrowIfError(status, "get text buffer size");

            // Allocate buffer and get the text
            var buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_text_at(_handle.DangerousGetHandle(), i, buffer, ref bufferSize);
                OpenVINOGenAIException.ThrowIfError(status, "get text");

                texts[i] = Marshal.PtrToStringAnsi(buffer) ?? string.Empty;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        return texts;
    }

    /// <summary>
    /// Loads the confidence scores from the native handle
    /// </summary>
    private float[] LoadScores()
    {
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_texts_size(_handle.DangerousGetHandle(), out var textsSize);
        OpenVINOGenAIException.ThrowIfError(status, "get texts size for scores");

        var scores = new float[textsSize];
        status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_scores(_handle.DangerousGetHandle(), scores, textsSize);
        OpenVINOGenAIException.ThrowIfError(status, "get scores");

        return scores;
    }

    /// <summary>
    /// Loads the timestamped chunks from the native handle
    /// </summary>
    private WhisperChunk[] LoadChunks()
    {
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_chunks_size(_handle.DangerousGetHandle(), out var chunksSize);
        OpenVINOGenAIException.ThrowIfError(status, "get chunks size");

        var chunks = new WhisperChunk[chunksSize];
        for (nuint i = 0; i < chunksSize; i++)
        {
            status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_chunk_at(_handle.DangerousGetHandle(), i, out var nativeChunk);
            OpenVINOGenAIException.ThrowIfError(status, "get chunk");

            chunks[i] = new WhisperChunk(nativeChunk.start_ts, nativeChunk.end_ts, nativeChunk.text ?? string.Empty);
        }

        return chunks;
    }

    /// <summary>
    /// Loads the performance metrics from the native handle
    /// </summary>
    private WhisperPerformanceMetrics LoadPerformanceMetrics()
    {
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_perf_metrics(_handle.DangerousGetHandle(), out var metricsHandle);
        OpenVINOGenAIException.ThrowIfError(status, "get performance metrics");

        return new WhisperPerformanceMetrics(metricsHandle);
    }

    /// <summary>
    /// Releases all resources used by the WhisperResult
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
            throw new ObjectDisposedException(nameof(WhisperResult));
    }
}

/// <summary>
/// Performance metrics for Whisper transcription
/// </summary>
public sealed class WhisperPerformanceMetrics : IDisposable
{
    private readonly IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WhisperPerformanceMetrics class
    /// </summary>
    /// <param name="handle">The native handle for the metrics</param>
    internal WhisperPerformanceMetrics(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the features extraction duration statistics
    /// </summary>
    public (float Mean, float StdDev) FeaturesExtractionDuration
    {
        get
        {
            ThrowIfDisposed();
            var status = GenAINativeMethods.ov_genai_whisper_perf_metrics_get_features_extraction_duration(_handle, out var mean, out var std);
            OpenVINOGenAIException.ThrowIfError(status, "get features extraction duration");
            return (mean, std);
        }
    }

    /// <summary>
    /// Returns a string representation of the metrics
    /// </summary>
    /// <returns>String representation of the performance metrics</returns>
    public override string ToString()
    {
        var extraction = FeaturesExtractionDuration;
        return $"Features Extraction: {extraction.Mean:F2}ms ± {extraction.StdDev:F2}ms";
    }

    /// <summary>
    /// Releases all resources used by the WhisperPerformanceMetrics
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            GenAINativeMethods.ov_genai_whisper_perf_metrics_free(_handle);
            _disposed = true;
        }
    }

    /// <summary>
    /// Throws if the object has been disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WhisperPerformanceMetrics));
    }
}