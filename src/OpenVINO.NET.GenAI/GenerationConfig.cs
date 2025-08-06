using Fluid.OpenVINO.GenAI.Exceptions;
using Fluid.OpenVINO.GenAI.Native;
using Fluid.OpenVINO.GenAI.SafeHandles;

namespace Fluid.OpenVINO.GenAI;

/// <summary>
/// Configuration for text generation parameters
/// </summary>
public sealed class GenerationConfig : IDisposable
{
    private readonly GenerationConfigSafeHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the GenerationConfig class
    /// </summary>
    public GenerationConfig()
    {
        // Ensure native libraries are loaded before any P/Invoke calls
        NativeLibraryLoader.EnsureLoaded();

        var status = GenAINativeMethods.ov_genai_generation_config_create(out var handle);
        OpenVINOGenAIException.ThrowIfError(status, "create generation config");
        _handle = new GenerationConfigSafeHandle(handle, true);
    }

    /// <summary>
    /// Initializes a new instance of the GenerationConfig class from a JSON file
    /// </summary>
    /// <param name="jsonPath">Path to the JSON configuration file</param>
    public GenerationConfig(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath))
            throw new ArgumentException("JSON path cannot be null or empty", nameof(jsonPath));

        // Ensure native libraries are loaded before any P/Invoke calls
        NativeLibraryLoader.EnsureLoaded();

        var status = GenAINativeMethods.ov_genai_generation_config_create_from_json(jsonPath, out var handle);
        OpenVINOGenAIException.ThrowIfError(status, "create generation config from JSON");
        _handle = new GenerationConfigSafeHandle(handle, true);
    }

    /// <summary>
    /// Internal constructor from existing handle
    /// </summary>
    /// <param name="handle">Existing native handle</param>
    internal GenerationConfig(GenerationConfigSafeHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the native handle
    /// </summary>
    internal IntPtr Handle => _handle.DangerousGetHandle();

    /// <summary>
    /// Gets the default generation configuration
    /// </summary>
    public static GenerationConfig Default => new();

    /// <summary>
    /// Sets the maximum number of tokens to generate
    /// </summary>
    /// <param name="maxNewTokens">Maximum number of tokens</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithMaxTokens(int maxNewTokens)
    {
        ThrowIfDisposed();
        if (maxNewTokens < 0)
            throw new ArgumentOutOfRangeException(nameof(maxNewTokens), "Max new tokens cannot be negative");

        var status = GenAINativeMethods.ov_genai_generation_config_set_max_new_tokens(_handle.DangerousGetHandle(), (nuint)maxNewTokens);
        OpenVINOGenAIException.ThrowIfError(status, "set max new tokens");
        return this;
    }

    /// <summary>
    /// Sets the maximum length of the generated sequence
    /// </summary>
    /// <param name="maxLength">Maximum sequence length</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithMaxLength(int maxLength)
    {
        ThrowIfDisposed();
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be negative");

        var status = GenAINativeMethods.ov_genai_generation_config_set_max_length(_handle.DangerousGetHandle(), (nuint)maxLength);
        OpenVINOGenAIException.ThrowIfError(status, "set max length");
        return this;
    }

    /// <summary>
    /// Sets the temperature for sampling
    /// </summary>
    /// <param name="temperature">Temperature value (typically 0.0 to 2.0)</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithTemperature(float temperature)
    {
        ThrowIfDisposed();
        if (temperature < 0)
            throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature cannot be negative");

        var status = GenAINativeMethods.ov_genai_generation_config_set_temperature(_handle.DangerousGetHandle(), temperature);
        OpenVINOGenAIException.ThrowIfError(status, "set temperature");
        return this;
    }

    /// <summary>
    /// Sets the top-p value for nucleus sampling
    /// </summary>
    /// <param name="topP">Top-p value (0.0 to 1.0)</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithTopP(float topP)
    {
        ThrowIfDisposed();
        if (topP < 0.0f || topP > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(topP), "Top-P must be between 0.0 and 1.0");

        var status = GenAINativeMethods.ov_genai_generation_config_set_top_p(_handle.DangerousGetHandle(), topP);
        OpenVINOGenAIException.ThrowIfError(status, "set top_p");
        return this;
    }

    /// <summary>
    /// Sets the top-k value for top-k sampling
    /// </summary>
    /// <param name="topK">Top-k value</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithTopK(int topK)
    {
        ThrowIfDisposed();
        if (topK < 0)
            throw new ArgumentOutOfRangeException(nameof(topK), "Top-K cannot be negative");

        var status = GenAINativeMethods.ov_genai_generation_config_set_top_k(_handle.DangerousGetHandle(), (nuint)topK);
        OpenVINOGenAIException.ThrowIfError(status, "set top_k");
        return this;
    }

    /// <summary>
    /// Sets whether to use sampling instead of greedy decoding
    /// </summary>
    /// <param name="doSample">Whether to use sampling</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithSampling(bool doSample)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_generation_config_set_do_sample(_handle.DangerousGetHandle(), doSample);
        OpenVINOGenAIException.ThrowIfError(status, "set do_sample");
        return this;
    }

    /// <summary>
    /// Sets the repetition penalty
    /// </summary>
    /// <param name="repetitionPenalty">Repetition penalty value (1.0 means no penalty)</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithRepetitionPenalty(float repetitionPenalty)
    {
        ThrowIfDisposed();
        if (repetitionPenalty < 0)
            throw new ArgumentOutOfRangeException(nameof(repetitionPenalty), "Repetition penalty cannot be negative");

        var status = GenAINativeMethods.ov_genai_generation_config_set_repetition_penalty(_handle.DangerousGetHandle(), repetitionPenalty);
        OpenVINOGenAIException.ThrowIfError(status, "set repetition penalty");
        return this;
    }

    /// <summary>
    /// Sets the presence penalty
    /// </summary>
    /// <param name="presencePenalty">Presence penalty value</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithPresencePenalty(float presencePenalty)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_generation_config_set_presence_penalty(_handle.DangerousGetHandle(), presencePenalty);
        OpenVINOGenAIException.ThrowIfError(status, "set presence penalty");
        return this;
    }

    /// <summary>
    /// Sets the frequency penalty
    /// </summary>
    /// <param name="frequencyPenalty">Frequency penalty value</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithFrequencyPenalty(float frequencyPenalty)
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_generation_config_set_frequency_penalty(_handle.DangerousGetHandle(), frequencyPenalty);
        OpenVINOGenAIException.ThrowIfError(status, "set frequency penalty");
        return this;
    }

    /// <summary>
    /// Sets stop strings that will terminate generation
    /// </summary>
    /// <param name="stopStrings">Array of stop strings</param>
    /// <returns>This configuration instance for fluent chaining</returns>
    public GenerationConfig WithStopStrings(params string[] stopStrings)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(stopStrings);

        var status = GenAINativeMethods.ov_genai_generation_config_set_stop_strings(
            _handle.DangerousGetHandle(),
            stopStrings,
            (nuint)stopStrings.Length);
        OpenVINOGenAIException.ThrowIfError(status, "set stop strings");
        return this;
    }

    /// <summary>
    /// Gets the maximum number of new tokens
    /// </summary>
    /// <returns>The maximum number of new tokens</returns>
    public int GetMaxNewTokens()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_generation_config_get_max_new_tokens(_handle.DangerousGetHandle(), out var maxNewTokens);
        OpenVINOGenAIException.ThrowIfError(status, "get max new tokens");
        return (int)maxNewTokens;
    }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    public void Validate()
    {
        ThrowIfDisposed();

        var status = GenAINativeMethods.ov_genai_generation_config_validate(_handle.DangerousGetHandle());
        OpenVINOGenAIException.ThrowIfError(status, "validate configuration");
    }

    /// <summary>
    /// Releases all resources used by the GenerationConfig
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
            throw new ObjectDisposedException(nameof(GenerationConfig));
    }
}
