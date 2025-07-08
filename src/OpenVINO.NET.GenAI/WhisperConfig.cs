using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Configuration for Whisper speech transcription
/// </summary>
public sealed class WhisperConfig : IDisposable
{
    private readonly WhisperConfigSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the default Whisper configuration
    /// </summary>
    public static WhisperConfig Default => new();

    /// <summary>
    /// Initializes a new instance of the WhisperConfig class
    /// </summary>
    public WhisperConfig()
    {
        var status = GenAINativeMethods.ov_genai_whisper_generation_config_create(out var handle);
        OpenVINOGenAIException.ThrowIfError(status, "create Whisper generation config");
        _handle = new WhisperConfigSafeHandle(handle, true);
    }

    /// <summary>
    /// Initializes a new instance of the WhisperConfig class with an existing handle
    /// </summary>
    /// <param name="handle">The existing handle</param>
    internal WhisperConfig(WhisperConfigSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the internal handle
    /// </summary>
    internal IntPtr Handle => _handle.DangerousGetHandle();

    /// <summary>
    /// Sets the language for transcription
    /// </summary>
    /// <param name="language">Language code (e.g., "en", "es", "fr")</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithLanguage(string language)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_language(_handle.DangerousGetHandle(), language);
        OpenVINOGenAIException.ThrowIfError(status, "set language");
        return this;
    }

    /// <summary>
    /// Sets the task type for Whisper processing
    /// </summary>
    /// <param name="task">Task type: "transcribe" or "translate"</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithTask(string task)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(task))
            throw new ArgumentException("Task cannot be null or empty", nameof(task));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_task(_handle.DangerousGetHandle(), task);
        OpenVINOGenAIException.ThrowIfError(status, "set task");
        return this;
    }

    /// <summary>
    /// Sets whether to return timestamps with transcription
    /// </summary>
    /// <param name="returnTimestamps">True to return timestamps, false otherwise</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithTimestamps(bool returnTimestamps)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_return_timestamps(_handle.DangerousGetHandle(), returnTimestamps);
        OpenVINOGenAIException.ThrowIfError(status, "set return timestamps");
        return this;
    }

    /// <summary>
    /// Sets the initial prompt to guide transcription
    /// </summary>
    /// <param name="initialPrompt">Initial prompt text</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithInitialPrompt(string initialPrompt)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(initialPrompt))
            throw new ArgumentException("Initial prompt cannot be null or empty", nameof(initialPrompt));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_initial_prompt(_handle.DangerousGetHandle(), initialPrompt);
        OpenVINOGenAIException.ThrowIfError(status, "set initial prompt");
        return this;
    }

    /// <summary>
    /// Sets hotwords to emphasize during transcription
    /// </summary>
    /// <param name="hotwords">Hotwords to emphasize</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithHotwords(string hotwords)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(hotwords))
            throw new ArgumentException("Hotwords cannot be null or empty", nameof(hotwords));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_hotwords(_handle.DangerousGetHandle(), hotwords);
        OpenVINOGenAIException.ThrowIfError(status, "set hotwords");
        return this;
    }

    /// <summary>
    /// Sets the maximum initial timestamp index
    /// </summary>
    /// <param name="maxInitialTimestampIndex">Maximum initial timestamp index</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithMaxInitialTimestampIndex(uint maxInitialTimestampIndex)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_max_initial_timestamp_index(_handle.DangerousGetHandle(), maxInitialTimestampIndex);
        OpenVINOGenAIException.ThrowIfError(status, "set max initial timestamp index");
        return this;
    }

    /// <summary>
    /// Sets the decoder start token ID
    /// </summary>
    /// <param name="decoderStartTokenId">Decoder start token ID</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithDecoderStartTokenId(long decoderStartTokenId)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_decoder_start_token_id(_handle.DangerousGetHandle(), decoderStartTokenId);
        OpenVINOGenAIException.ThrowIfError(status, "set decoder start token ID");
        return this;
    }

    /// <summary>
    /// Sets tokens to suppress during generation
    /// </summary>
    /// <param name="suppressTokens">Array of token IDs to suppress</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithSuppressTokens(long[] suppressTokens)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(suppressTokens);

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_suppress_tokens(
            _handle.DangerousGetHandle(), suppressTokens, (nuint)suppressTokens.Length);
        OpenVINOGenAIException.ThrowIfError(status, "set suppress tokens");
        return this;
    }

    /// <summary>
    /// Sets tokens to suppress at the beginning of generation
    /// </summary>
    /// <param name="beginSuppressTokens">Array of token IDs to suppress at beginning</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig WithBeginSuppressTokens(long[] beginSuppressTokens)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(beginSuppressTokens);

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_begin_suppress_tokens(
            _handle.DangerousGetHandle(), beginSuppressTokens, (nuint)beginSuppressTokens.Length);
        OpenVINOGenAIException.ThrowIfError(status, "set begin suppress tokens");
        return this;
    }

    /// <summary>
    /// Convenience method to set up for transcription in English
    /// </summary>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig ForTranscriptionEnglish()
    {
        return WithLanguage("en").WithTask("transcribe").WithTimestamps(true);
    }

    /// <summary>
    /// Convenience method to set up for translation to English
    /// </summary>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig ForTranslationToEnglish()
    {
        return WithTask("translate").WithTimestamps(true);
    }

    /// <summary>
    /// Convenience method to set up for transcription in a specific language
    /// </summary>
    /// <param name="language">Language code (e.g., "es", "fr", "de")</param>
    /// <returns>This WhisperConfig instance for method chaining</returns>
    public WhisperConfig ForTranscriptionLanguage(string language)
    {
        return WithLanguage(language).WithTask("transcribe").WithTimestamps(true);
    }

    /// <summary>
    /// Releases all resources used by the WhisperConfig
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
            throw new ObjectDisposedException(nameof(WhisperConfig));
    }
}