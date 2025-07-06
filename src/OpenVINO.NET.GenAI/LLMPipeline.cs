using System.Runtime.CompilerServices;
using System.Threading.Channels;
using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Large Language Model pipeline for text generation
/// </summary>
public sealed class LLMPipeline : IDisposable
{
    private readonly LLMPipelineSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the LLMPipeline class
    /// </summary>
    /// <param name="modelPath">Path to the model directory</param>
    /// <param name="device">Device to run on (e.g., "CPU", "GPU")</param>
    public LLMPipeline(string modelPath, string device = "CPU")
    {
        if (string.IsNullOrEmpty(modelPath))
            throw new ArgumentException("Model path cannot be null or empty", nameof(modelPath));
        if (string.IsNullOrEmpty(device))
            throw new ArgumentException("Device cannot be null or empty", nameof(device));

        var status = GenAINativeMethods.ov_genai_llm_pipeline_create(
            modelPath, 
            device, 
            0, // No properties for now
            out var handle);
        
        OpenVINOGenAIException.ThrowIfError(status, "create LLM pipeline");
        _handle = new LLMPipelineSafeHandle(handle, true);
    }

    /// <summary>
    /// Generates text synchronously
    /// </summary>
    /// <param name="prompt">The input prompt</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <returns>The generation result</returns>
    public GenerationResult Generate(string prompt, GenerationConfig? config = null)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(prompt))
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

        var configHandle = config?.Handle ?? IntPtr.Zero;
        
        var status = GenAINativeMethods.ov_genai_llm_pipeline_generate(
            _handle.DangerousGetHandle(),
            prompt,
            configHandle,
            IntPtr.Zero, // No streamer
            out var resultsHandle);
        
        OpenVINOGenAIException.ThrowIfError(status, "generate text");
        
        return new GenerationResult(new DecodedResultsSafeHandle(resultsHandle, true));
    }

    /// <summary>
    /// Generates text asynchronously
    /// </summary>
    /// <param name="prompt">The input prompt</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generation result</returns>
    public async Task<GenerationResult> GenerateAsync(
        string prompt, 
        GenerationConfig? config = null, 
        CancellationToken cancellationToken = default)
    {
        // Run the synchronous generation on a background thread
        return await Task.Run(() => Generate(prompt, config), cancellationToken);
    }

    /// <summary>
    /// Generates text with streaming output
    /// </summary>
    /// <param name="prompt">The input prompt</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of generated tokens</returns>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string prompt,
        GenerationConfig? config = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(prompt))
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

        var channel = Channel.CreateUnbounded<string>();
        var writer = channel.Writer;
        var reader = channel.Reader;

        var callbackData = new StreamingCallbackData(writer, cancellationToken);
        var gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(callbackData, System.Runtime.InteropServices.GCHandleType.Normal);

        try
        {
            var streamerCallback = new streamer_callback
            {
                callback_func = StreamingCallbackFunction.FunctionPointer,
                args = System.Runtime.InteropServices.GCHandle.ToIntPtr(gcHandle)
            };

            var configHandle = config?.Handle ?? IntPtr.Zero;

            // Start generation in a background task
            var generationTask = Task.Run(() =>
            {
                try
                {
                    var streamerPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(
                        System.Runtime.InteropServices.Marshal.SizeOf<streamer_callback>());
                    
                    try
                    {
                        System.Runtime.InteropServices.Marshal.StructureToPtr(streamerCallback, streamerPtr, false);

                        var status = GenAINativeMethods.ov_genai_llm_pipeline_generate(
                            _handle.DangerousGetHandle(),
                            prompt,
                            configHandle,
                            streamerPtr,
                            out _); // We don't need the results when streaming

                        if (status != ov_status_e.OK)
                        {
                            callbackData.SetError(new OpenVINOGenAIException(status, "stream generation failed"));
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

            // Yield tokens as they arrive
            await foreach (var token in reader.ReadAllAsync(cancellationToken))
            {
                yield return token;
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
    /// Starts a chat session
    /// </summary>
    public void StartChat()
    {
        ThrowIfDisposed();
        
        var status = GenAINativeMethods.ov_genai_llm_pipeline_start_chat(_handle.DangerousGetHandle());
        OpenVINOGenAIException.ThrowIfError(status, "start chat");
    }

    /// <summary>
    /// Finishes a chat session
    /// </summary>
    public void FinishChat()
    {
        ThrowIfDisposed();
        
        var status = GenAINativeMethods.ov_genai_llm_pipeline_finish_chat(_handle.DangerousGetHandle());
        OpenVINOGenAIException.ThrowIfError(status, "finish chat");
    }

    /// <summary>
    /// Gets the current generation configuration
    /// </summary>
    /// <returns>The current generation configuration</returns>
    public GenerationConfig GetGenerationConfig()
    {
        ThrowIfDisposed();
        
        var status = GenAINativeMethods.ov_genai_llm_pipeline_get_generation_config(
            _handle.DangerousGetHandle(), 
            out var configHandle);
        
        OpenVINOGenAIException.ThrowIfError(status, "get generation config");
        
        return new GenerationConfig(new GenerationConfigSafeHandle(configHandle, true));
    }

    /// <summary>
    /// Sets the generation configuration
    /// </summary>
    /// <param name="config">The generation configuration to set</param>
    public void SetGenerationConfig(GenerationConfig config)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(config);
        
        var status = GenAINativeMethods.ov_genai_llm_pipeline_set_generation_config(
            _handle.DangerousGetHandle(), 
            config.Handle);
        
        OpenVINOGenAIException.ThrowIfError(status, "set generation config");
    }

    /// <summary>
    /// Releases all resources used by the LLMPipeline
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
            throw new ObjectDisposedException(nameof(LLMPipeline));
    }
}

/// <summary>
/// Data for streaming callback
/// </summary>
internal sealed class StreamingCallbackData
{
    private readonly ChannelWriter<string> _writer;
    private readonly CancellationToken _cancellationToken;
    private Exception? _error;

    public StreamingCallbackData(ChannelWriter<string> writer, CancellationToken cancellationToken)
    {
        _writer = writer;
        _cancellationToken = cancellationToken;
    }

    public void WriteToken(string token)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _writer.TryWrite(token);
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
/// Static class to hold the streaming callback function
/// </summary>
internal static class StreamingCallbackFunction
{
    public static readonly IntPtr FunctionPointer = 
        System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate<StreamerCallbackFunc>(CallbackImpl);

    private static ov_genai_streamming_status_e CallbackImpl(string str, IntPtr args)
    {
        try
        {
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr(args);
            var callbackData = (StreamingCallbackData)gcHandle.Target!;

            if (callbackData.IsCancellationRequested)
            {
                return ov_genai_streamming_status_e.CANCEL;
            }

            callbackData.WriteToken(str);
            return ov_genai_streamming_status_e.RUNNING;
        }
        catch (Exception ex)
        {
            // Log error if needed, but can't throw from callback
            System.Diagnostics.Debug.WriteLine($"Streaming callback error: {ex}");
            return ov_genai_streamming_status_e.STOP;
        }
    }
}