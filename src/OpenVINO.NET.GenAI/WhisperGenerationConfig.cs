using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using OpenVINO.NET.GenAI.SafeHandles;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Configuration for Whisper speech recognition parameters
/// </summary>
public sealed class WhisperGenerationConfig : IDisposable
{
    private readonly WhisperGenerationConfigSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WhisperGenerationConfig class
    /// </summary>
    public WhisperGenerationConfig()
    {
        // Ensure native libraries are loaded before any P/Invoke calls
        NativeLibraryLoader.EnsureLoaded();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_create(out var handle);
        OpenVINOGenAIException.ThrowIfError(status, "create whisper generation config");
        _handle = new WhisperGenerationConfigSafeHandle(handle, true);
    }

    /// <summary>
    /// Initializes a new instance of the WhisperGenerationConfig class from a JSON file
    /// </summary>
    /// <param name="jsonPath">Path to the JSON configuration file</param>
    public WhisperGenerationConfig(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath))
            throw new ArgumentException("JSON path cannot be null or empty", nameof(jsonPath));

        // Ensure native libraries are loaded before any P/Invoke calls
        NativeLibraryLoader.EnsureLoaded();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_create_from_json(jsonPath, out var handle);
        OpenVINOGenAIException.ThrowIfError(status, "create whisper generation config from JSON");
        _handle = new WhisperGenerationConfigSafeHandle(handle, true);
    }

    /// <summary>
    /// Internal constructor from existing handle
    /// </summary>
    /// <param name="handle">Existing native handle</param>
    internal WhisperGenerationConfig(WhisperGenerationConfigSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the native handle
    /// </summary>
    internal IntPtr Handle => _handle.DangerousGetHandle();

    /// <summary>
    /// Gets the default whisper generation configuration
    /// </summary>
    public static WhisperGenerationConfig Default => new();

    /// <summary>
    /// Sets the language for transcription
    /// </summary>
    /// <param name="language">Language code (e.g., "en", "es", "fr")</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public WhisperGenerationConfig WithLanguage(string language)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_language(_handle.DangerousGetHandle(), language);
        OpenVINOGenAIException.ThrowIfError(status, "set language");
        return this;
    }

    /// <summary>
    /// Sets the task type
    /// </summary>
    /// <param name="task">Task type: "transcribe" or "translate"</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public WhisperGenerationConfig WithTask(WhisperTask task)
    {
        ThrowIfDisposed();
        var taskString = task == WhisperTask.Transcribe ? "transcribe" : "translate";

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_task(_handle.DangerousGetHandle(), taskString);
        OpenVINOGenAIException.ThrowIfError(status, "set task");
        return this;
    }

    /// <summary>
    /// Sets whether to return timestamps with the transcription
    /// </summary>
    /// <param name="returnTimestamps">True to return timestamps, false otherwise</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public WhisperGenerationConfig WithTimestamps(bool returnTimestamps = true)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_return_timestamps(_handle.DangerousGetHandle(), returnTimestamps);
        OpenVINOGenAIException.ThrowIfError(status, "set return timestamps");
        return this;
    }

    /// <summary>
    /// Sets the initial prompt to guide the transcription
    /// </summary>
    /// <param name="prompt">Initial prompt text</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public WhisperGenerationConfig WithInitialPrompt(string prompt)
    {
        ThrowIfDisposed();
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_initial_prompt(_handle.DangerousGetHandle(), prompt);
        OpenVINOGenAIException.ThrowIfError(status, "set initial prompt");
        return this;
    }

    /// <summary>
    /// Sets hotwords to emphasize during transcription
    /// </summary>
    /// <param name="hotwords">Comma-separated list of hotwords</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public WhisperGenerationConfig WithHotwords(string hotwords)
    {
        ThrowIfDisposed();
        if (hotwords == null)
            throw new ArgumentNullException(nameof(hotwords));

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_set_hotwords(_handle.DangerousGetHandle(), hotwords);
        OpenVINOGenAIException.ThrowIfError(status, "set hotwords");
        return this;
    }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    public void Validate()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_whisper_generation_config_validate(_handle.DangerousGetHandle());
        OpenVINOGenAIException.ThrowIfError(status, "validate whisper generation config");
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
            throw new ObjectDisposedException(nameof(WhisperGenerationConfig));
    }
}

/// <summary>
/// Whisper task types
/// </summary>
public enum WhisperTask
{
    /// <summary>
    /// Transcribe audio in the original language
    /// </summary>
    Transcribe,

    /// <summary>
    /// Translate audio to English
    /// </summary>
    Translate
}