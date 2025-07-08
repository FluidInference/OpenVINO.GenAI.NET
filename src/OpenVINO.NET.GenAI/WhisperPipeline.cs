using System.Runtime.CompilerServices;
using System.Threading.Channels;
using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Whisper pipeline for speech transcription and translation
/// </summary>
public sealed class WhisperPipeline : IDisposable
{
    private readonly WhisperPipelineSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WhisperPipeline class
    /// </summary>
    /// <param name="modelPath">Path to the Whisper model directory</param>
    /// <param name="device">Device to run on (e.g., "CPU", "GPU", "NPU")</param>
    public WhisperPipeline(string modelPath, string device = "CPU")
    {
        if (string.IsNullOrEmpty(modelPath))
            throw new ArgumentException("Model path cannot be null or empty", nameof(modelPath));
        if (string.IsNullOrEmpty(device))
            throw new ArgumentException("Device cannot be null or empty", nameof(device));

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_create(
            modelPath,
            device,
            0, // No properties for now
            out var handle);

        OpenVINOGenAIException.ThrowIfError(status, "create Whisper pipeline");
        _handle = new WhisperPipelineSafeHandle(handle, true);
    }

    /// <summary>
    /// Transcribes audio synchronously
    /// </summary>
    /// <param name="audioData">Raw audio data (16kHz, float32)</param>
    /// <param name="config">Whisper configuration (optional)</param>
    /// <returns>The transcription result</returns>
    public WhisperResult Generate(float[] audioData, WhisperConfig? config = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(audioData);

        ValidateAudioData(audioData);

        var configHandle = config?.Handle ?? IntPtr.Zero;

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_generate(
            _handle.DangerousGetHandle(),
            audioData,
            (nuint)audioData.Length,
            configHandle,
            IntPtr.Zero, // No streamer
            out var resultsHandle);

        OpenVINOGenAIException.ThrowIfError(status, "generate transcription");

        return new WhisperResult(new WhisperResultsSafeHandle(resultsHandle, true));
    }

    /// <summary>
    /// Transcribes audio asynchronously
    /// </summary>
    /// <param name="audioData">Raw audio data (16kHz, float32)</param>
    /// <param name="config">Whisper configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transcription result</returns>
    public async Task<WhisperResult> GenerateAsync(
        float[] audioData,
        WhisperConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        // Run the synchronous generation on a background thread
        return await Task.Run(() => Generate(audioData, config), cancellationToken);
    }

    /// <summary>
    /// Transcribes audio with streaming output for real-time processing
    /// </summary>
    /// <param name="audioData">Raw audio data (16kHz, float32)</param>
    /// <param name="config">Whisper configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of transcription chunks</returns>
    public async IAsyncEnumerable<WhisperChunk> GenerateStreamAsync(
        float[] audioData,
        WhisperConfig? config = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(audioData);

        ValidateAudioData(audioData);

        var channel = Channel.CreateUnbounded<WhisperChunk>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        var callbackData = new WhisperStreamingCallbackData(writer, cancellationToken);
        var gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(callbackData, System.Runtime.InteropServices.GCHandleType.Normal);

        try
        {
            var streamerCallback = new whisper_streaming_callback
            {
                callback_func = WhisperStreamingCallbackFunction.FunctionPointer,
                args = System.Runtime.InteropServices.GCHandle.ToIntPtr(gcHandle)
            };

            var configHandle = config?.Handle ?? IntPtr.Zero;

            // Start generation in a background task
            var generationTask = Task.Run(() =>
            {
                try
                {
                    var streamerPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(
                        System.Runtime.InteropServices.Marshal.SizeOf<whisper_streaming_callback>());

                    try
                    {
                        System.Runtime.InteropServices.Marshal.StructureToPtr(streamerCallback, streamerPtr, false);

                        var status = GenAINativeMethods.ov_genai_whisper_pipeline_generate(
                            _handle.DangerousGetHandle(),
                            audioData,
                            (nuint)audioData.Length,
                            configHandle,
                            streamerPtr,
                            out _); // We don't need the results when streaming

                        if (status != ov_status_e.OK)
                        {
                            callbackData.SetError(new OpenVINOGenAIException(status, "stream transcription failed"));
                        }
                    }
                    finally
                    {
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(streamerPtr);
                    }
                }
                catch (Exception ex)
                {
                    callbackData.SetError(ex);
                }
                finally
                {
                    writer.TryComplete();
                }
            }, cancellationToken);

            // Yield chunks as they arrive
            await foreach (var chunk in reader.ReadAllAsync(cancellationToken))
            {
                yield return chunk;
            }

            // Wait for generation to complete and check for errors
            await generationTask;
            callbackData.ThrowIfError();
        }
        finally
        {
            if (gcHandle.IsAllocated)
            {
                gcHandle.Free();
            }
        }
    }

    /// <summary>
    /// Gets the current generation configuration
    /// </summary>
    /// <returns>The current Whisper configuration</returns>
    public WhisperConfig GetGenerationConfig()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_get_generation_config(
            _handle.DangerousGetHandle(),
            out var configHandle);

        OpenVINOGenAIException.ThrowIfError(status, "get Whisper generation config");

        return new WhisperConfig(new WhisperConfigSafeHandle(configHandle, true));
    }

    /// <summary>
    /// Sets the generation configuration
    /// </summary>
    /// <param name="config">The Whisper configuration to set</param>
    public void SetGenerationConfig(WhisperConfig config)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(config);

        var status = GenAINativeMethods.ov_genai_whisper_pipeline_set_generation_config(
            _handle.DangerousGetHandle(),
            config.Handle);

        OpenVINOGenAIException.ThrowIfError(status, "set Whisper generation config");
    }

    /// <summary>
    /// Validates audio data format and constraints
    /// </summary>
    /// <param name="audioData">Audio data to validate</param>
    private static void ValidateAudioData(float[] audioData)
    {
        if (audioData.Length == 0)
            throw new ArgumentException("Audio data cannot be empty", nameof(audioData));

        // Check for maximum audio length (30 seconds at 16kHz = 480,000 samples)
        const int maxSamples = 480000;
        if (audioData.Length > maxSamples)
            throw new ArgumentException($"Audio data is too long. Maximum length is {maxSamples} samples (30 seconds at 16kHz)", nameof(audioData));

        // Check for invalid values (NaN, Infinity)
        for (int i = 0; i < audioData.Length; i++)
        {
            if (float.IsNaN(audioData[i]) || float.IsInfinity(audioData[i]))
                throw new ArgumentException($"Audio data contains invalid value at index {i}: {audioData[i]}", nameof(audioData));
        }
    }

    /// <summary>
    /// Releases all resources used by the WhisperPipeline
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
            throw new ObjectDisposedException(nameof(WhisperPipeline));
    }
}

/// <summary>
/// Data for Whisper streaming callback
/// </summary>
internal sealed class WhisperStreamingCallbackData
{
    private readonly ChannelWriter<WhisperChunk> _writer;
    private readonly CancellationToken _cancellationToken;
    private Exception? _error;

    public WhisperStreamingCallbackData(ChannelWriter<WhisperChunk> writer, CancellationToken cancellationToken)
    {
        _writer = writer;
        _cancellationToken = cancellationToken;
    }

    public void WriteChunk(WhisperChunk chunk)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _writer.TryWrite(chunk);
    }

    public void SetError(Exception error)
    {
        _error = error;
    }

    public void ThrowIfError()
    {
        if (_error != null)
        {
            throw _error;
        }
    }

    public bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;
}

/// <summary>
/// Static class to hold the Whisper streaming callback function
/// </summary>
internal static class WhisperStreamingCallbackFunction
{
    public static readonly IntPtr FunctionPointer =
        System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate<WhisperStreamingCallbackFunc>(CallbackImpl);

    private static ov_genai_streamming_status_e CallbackImpl(whisper_result_chunk chunk, IntPtr args)
    {
        try
        {
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(args);
            var callbackData = (WhisperStreamingCallbackData)gcHandle.Target!;

            if (callbackData.IsCancellationRequested)
            {
                return ov_genai_streamming_status_e.CANCEL;
            }

            var whisperChunk = new WhisperChunk(chunk.start_ts, chunk.end_ts, chunk.text ?? string.Empty);
            callbackData.WriteChunk(whisperChunk);
            return ov_genai_streamming_status_e.RUNNING;
        }
        catch (Exception ex)
        {
            // Log error if needed, but can't throw from callback
            System.Diagnostics.Debug.WriteLine($"Whisper streaming callback error: {ex}");
            return ov_genai_streamming_status_e.STOP;
        }
    }
}