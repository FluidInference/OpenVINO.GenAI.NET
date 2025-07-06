namespace OpenVINO.NET.GenAI;

/// <summary>
/// Represents a chat session with automatic resource management
/// </summary>
public sealed class ChatSession : IDisposable
{
    private readonly LLMPipeline _pipeline;
    private bool _disposed;
    private bool _sessionStarted;

    /// <summary>
    /// Initializes a new instance of the ChatSession class
    /// </summary>
    /// <param name="pipeline">The LLM pipeline to use</param>
    internal ChatSession(LLMPipeline pipeline)
    {
        _pipeline = pipeline;
        _pipeline.StartChat();
        _sessionStarted = true;
    }

    /// <summary>
    /// Sends a message and gets a response
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <returns>The response from the model</returns>
    public GenerationResult SendMessage(string message, GenerationConfig? config = null)
    {
        ThrowIfDisposed();
        return _pipeline.Generate(message, config);
    }

    /// <summary>
    /// Sends a message and gets a response asynchronously
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the model</returns>
    public async Task<GenerationResult> SendMessageAsync(
        string message, 
        GenerationConfig? config = null, 
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _pipeline.GenerateAsync(message, config, cancellationToken);
    }

    /// <summary>
    /// Sends a message and gets a streaming response
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="config">Generation configuration (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response tokens</returns>
    public IAsyncEnumerable<string> SendMessageStreamAsync(
        string message,
        GenerationConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _pipeline.GenerateStreamAsync(message, config, cancellationToken);
    }

    /// <summary>
    /// Releases all resources used by the ChatSession
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_sessionStarted)
            {
                try
                {
                    _pipeline.FinishChat();
                }
                catch
                {
                    // Ignore errors when finishing chat during disposal
                }
                _sessionStarted = false;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Throws if the object has been disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChatSession));
    }
}

/// <summary>
/// Extension methods for LLMPipeline
/// </summary>
public static class LLMPipelineExtensions
{
    /// <summary>
    /// Starts a new chat session
    /// </summary>
    /// <param name="pipeline">The LLM pipeline</param>
    /// <returns>A new chat session</returns>
    public static ChatSession StartChatSession(this LLMPipeline pipeline)
    {
        return new ChatSession(pipeline);
    }
}