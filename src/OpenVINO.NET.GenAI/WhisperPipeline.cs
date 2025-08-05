using System.Runtime.InteropServices;
using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Whisper pipeline for speech recognition and translation
/// </summary>
public sealed class WhisperPipeline : IDisposable
{
    private readonly WhisperPipelineSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WhisperPipeline class
    /// </summary>
    /// <param name="modelPath">Path to the Whisper model directory</param>
    /// <param name="device">Device to run on (e.g., "CPU", "GPU")</param>
    public WhisperPipeline(string modelPath, string device = "CPU")
    {
        if (string.IsNullOrEmpty(modelPath))
            throw new ArgumentException("Model path cannot be null or empty", nameof(modelPath));
        if (string.IsNullOrEmpty(device))
            throw new ArgumentException("Device cannot be null or empty", nameof(device));

        // Ensure native libraries are loaded before any P/Invoke calls
        NativeLibraryLoader.EnsureLoaded();

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_create(
            modelPath,
            device,
            0, // No properties for now
            out var handle);

        OpenVINOGenAIException.ThrowIfError(status, "create Whisper pipeline");
        _handle = new WhisperPipelineSafeHandle(handle, true);
    }

    /// <summary>
    /// Generates transcription from raw audio data
    /// </summary>
    /// <param name="audioData">Raw audio data as float array (16kHz, mono, normalized to [-1, 1])</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <returns>List of decoded results</returns>
    public IReadOnlyList<WhisperDecodedResult> Generate(float[] audioData, WhisperGenerationConfig? config = null)
    {
        ThrowIfDisposed();
        if (audioData == null)
            throw new ArgumentNullException(nameof(audioData));
        if (audioData.Length == 0)
            throw new ArgumentException("Audio data cannot be empty", nameof(audioData));

        var configHandle = config?.Handle ?? IntPtr.Zero;

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_generate(
            _handle.DangerousGetHandle(),
            audioData,
            (nuint)audioData.Length,
            configHandle,
            out var resultsHandle);

        OpenVINOGenAIException.ThrowIfError(status, "generate transcription");

        using var results = new WhisperDecodedResultsSafeHandle(resultsHandle, true);
        return ExtractResults(results);
    }

    /// <summary>
    /// Generates transcription from raw audio data asynchronously
    /// </summary>
    /// <param name="audioData">Raw audio data as float array (16kHz, mono, normalized to [-1, 1])</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of decoded results</returns>
    public async Task<IReadOnlyList<WhisperDecodedResult>> GenerateAsync(
        float[] audioData,
        WhisperGenerationConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        // Run the synchronous generation on a background thread
        return await Task.Run(() => Generate(audioData, config), cancellationToken);
    }

    /// <summary>
    /// Transcribes audio file
    /// </summary>
    /// <param name="audioFilePath">Path to audio file (WAV format recommended)</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <returns>List of decoded results</returns>
    public IReadOnlyList<WhisperDecodedResult> TranscribeFile(string audioFilePath, WhisperGenerationConfig? config = null)
    {
        if (string.IsNullOrEmpty(audioFilePath))
            throw new ArgumentException("Audio file path cannot be null or empty", nameof(audioFilePath));

        var audioData = AudioUtils.LoadAudioFile(audioFilePath);
        return Generate(audioData, config);
    }

    /// <summary>
    /// Transcribes audio file asynchronously
    /// </summary>
    /// <param name="audioFilePath">Path to audio file (WAV format recommended)</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of decoded results</returns>
    public async Task<IReadOnlyList<WhisperDecodedResult>> TranscribeFileAsync(
        string audioFilePath,
        WhisperGenerationConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(audioFilePath))
            throw new ArgumentException("Audio file path cannot be null or empty", nameof(audioFilePath));

        var audioData = await AudioUtils.LoadAudioFileAsync(audioFilePath, cancellationToken);
        return await GenerateAsync(audioData, config, cancellationToken);
    }

    /// <summary>
    /// Gets the current generation configuration
    /// </summary>
    /// <returns>The current generation configuration</returns>
    public WhisperGenerationConfig GetGenerationConfig()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_get_generation_config(
            _handle.DangerousGetHandle(),
            out var configHandle);

        OpenVINOGenAIException.ThrowIfError(status, "get generation config");
        return new WhisperGenerationConfig(new WhisperGenerationConfigSafeHandle(configHandle, true));
    }

    /// <summary>
    /// Sets the generation configuration
    /// </summary>
    /// <param name="config">The generation configuration to set</param>
    public void SetGenerationConfig(WhisperGenerationConfig config)
    {
        ThrowIfDisposed();
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_set_generation_config(
            _handle.DangerousGetHandle(),
            config.Handle);

        OpenVINOGenAIException.ThrowIfError(status, "set generation config");
    }

    private IReadOnlyList<WhisperDecodedResult> ExtractResults(WhisperDecodedResultsSafeHandle results)
    {
        var handle = results.DangerousGetHandle();

        // Get number of results
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_texts_count(handle, out var count);
        OpenVINOGenAIException.ThrowIfError(status, "get texts count");

        var resultList = new List<WhisperDecodedResult>((int)count);

        for (nuint i = 0; i < count; i++)
        {
            // Get text size first
            nuint textSize = 0;
            status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_text_at(handle, i, IntPtr.Zero, ref textSize);
            if (status != ov_status_e.OK && status != ov_status_e.OUT_OF_BOUNDS)
            {
                OpenVINOGenAIException.ThrowIfError(status, "get text size");
            }

            // Allocate buffer and get text
            var textBuffer = Marshal.AllocHGlobal((int)textSize);
            try
            {
                status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_text_at(handle, i, textBuffer, ref textSize);
                OpenVINOGenAIException.ThrowIfError(status, "get text");

                var text = Marshal.PtrToStringAnsi(textBuffer) ?? string.Empty;

                // Get score
                status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_score_at(handle, i, out var score);
                OpenVINOGenAIException.ThrowIfError(status, "get score");

                // Check if chunks are available
                status = GenAINativeMethods.ov_genai_whisper_decoded_results_has_chunks(handle, out var hasChunks);
                OpenVINOGenAIException.ThrowIfError(status, "check chunks");

                List<WhisperChunk>? chunks = null;
                if (hasChunks)
                {
                    chunks = ExtractChunks(results);
                }

                resultList.Add(new WhisperDecodedResult(text, score, chunks));
            }
            finally
            {
                Marshal.FreeHGlobal(textBuffer);
            }
        }

        return resultList;
    }

    private List<WhisperChunk> ExtractChunks(WhisperDecodedResultsSafeHandle results)
    {
        var handle = results.DangerousGetHandle();

        // Get number of chunks
        var status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_chunks_count(handle, out var count);
        OpenVINOGenAIException.ThrowIfError(status, "get chunks count");

        var chunks = new List<WhisperChunk>((int)count);

        for (nuint i = 0; i < count; i++)
        {
            // Get chunk
            status = GenAINativeMethods.ov_genai_whisper_decoded_results_get_chunk_at(handle, i, out var chunkHandle);
            OpenVINOGenAIException.ThrowIfError(status, "get chunk");

            using var chunk = new WhisperDecodedResultChunkSafeHandle(chunkHandle, true);
            var chunkPtr = chunk.DangerousGetHandle();

            // Get timestamps
            status = GenAINativeMethods.ov_genai_whisper_decoded_result_chunk_get_start_ts(chunkPtr, out var startTime);
            OpenVINOGenAIException.ThrowIfError(status, "get start timestamp");

            status = GenAINativeMethods.ov_genai_whisper_decoded_result_chunk_get_end_ts(chunkPtr, out var endTime);
            OpenVINOGenAIException.ThrowIfError(status, "get end timestamp");

            // Get text size first
            nuint textSize = 0;
            status = GenAINativeMethods.ov_genai_whisper_decoded_result_chunk_get_text(chunkPtr, IntPtr.Zero, ref textSize);
            if (status != ov_status_e.OK && status != ov_status_e.OUT_OF_BOUNDS)
            {
                OpenVINOGenAIException.ThrowIfError(status, "get chunk text size");
            }

            // Allocate buffer and get text
            var textBuffer = Marshal.AllocHGlobal((int)textSize);
            try
            {
                status = GenAINativeMethods.ov_genai_whisper_decoded_result_chunk_get_text(chunkPtr, textBuffer, ref textSize);
                OpenVINOGenAIException.ThrowIfError(status, "get chunk text");

                var text = Marshal.PtrToStringAnsi(textBuffer) ?? string.Empty;
                chunks.Add(new WhisperChunk(startTime, endTime, text));
            }
            finally
            {
                Marshal.FreeHGlobal(textBuffer);
            }
        }

        return chunks;
    }

    /// <summary>
    /// Disposes the native resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WhisperPipeline));
    }
}