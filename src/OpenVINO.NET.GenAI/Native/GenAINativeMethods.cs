using System.Runtime.InteropServices;

namespace OpenVINO.NET.GenAI.Native;

/// <summary>
/// P/Invoke declarations for OpenVINO GenAI C API
/// </summary>
internal static class GenAINativeMethods
{
    // Use standard library name without extension - .NET will find the appropriate file (.dll on Windows, .so on Linux)  
    // This matches the official OpenVINO GenAI C API library name
    private const string DllName = "openvino_genai_c";

    #region LLM Pipeline Methods

    /// <summary>
    /// Create LLM pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_llm_pipeline_create(
        [MarshalAs(UnmanagedType.LPStr)] string models_path,
        [MarshalAs(UnmanagedType.LPStr)] string device,
        nuint property_args_size,
        [Out] out IntPtr pipe);

    /// <summary>
    /// Free LLM pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_llm_pipeline_free(IntPtr pipe);

    /// <summary>
    /// Generate text using LLM pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_llm_pipeline_generate(
        IntPtr pipe,
        [MarshalAs(UnmanagedType.LPStr)] string inputs,
        IntPtr config,
        IntPtr streamer,
        [Out] out IntPtr results);

    /// <summary>
    /// Start chat session
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_llm_pipeline_start_chat(IntPtr pipe);

    /// <summary>
    /// Finish chat session
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_llm_pipeline_finish_chat(IntPtr pipe);

    /// <summary>
    /// Get generation config from pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_llm_pipeline_get_generation_config(
        IntPtr pipe,
        [Out] out IntPtr config);

    /// <summary>
    /// Set generation config to pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_llm_pipeline_set_generation_config(
        IntPtr pipe,
        IntPtr config);

    #endregion

    #region Generation Config Methods

    /// <summary>
    /// Create generation config
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_create([Out] out IntPtr config);

    /// <summary>
    /// Create generation config from JSON
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_generation_config_create_from_json(
        [MarshalAs(UnmanagedType.LPStr)] string json_path,
        [Out] out IntPtr config);

    /// <summary>
    /// Free generation config
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_generation_config_free(IntPtr config);

    /// <summary>
    /// Set max new tokens
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_max_new_tokens(
        IntPtr config, nuint value);

    /// <summary>
    /// Set max length
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_max_length(
        IntPtr config, nuint value);

    /// <summary>
    /// Set temperature
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_temperature(
        IntPtr config, float value);

    /// <summary>
    /// Set top_p
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_top_p(
        IntPtr config, float value);

    /// <summary>
    /// Set top_k
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_top_k(
        IntPtr config, nuint value);

    /// <summary>
    /// Set do_sample flag
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_do_sample(
        IntPtr config, [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Set repetition penalty
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_repetition_penalty(
        IntPtr config, float value);

    /// <summary>
    /// Set presence penalty
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_presence_penalty(
        IntPtr config, float value);

    /// <summary>
    /// Set frequency penalty
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_frequency_penalty(
        IntPtr config, float value);

    /// <summary>
    /// Set stop strings
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_stop_strings(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] strings,
        nuint count);

    /// <summary>
    /// Get max new tokens
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_get_max_new_tokens(
        IntPtr config, [Out] out nuint max_new_tokens);

    /// <summary>
    /// Set minimum new tokens
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_min_new_tokens(
        IntPtr config, nuint value);

    /// <summary>
    /// Set ignore EOS flag
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_ignore_eos(
        IntPtr config, [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Set echo flag
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_echo(
        IntPtr config, [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Set number of beams for beam search
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_num_beams(
        IntPtr config, nuint value);

    /// <summary>
    /// Set length penalty for beam search
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_length_penalty(
        IntPtr config, float value);

    /// <summary>
    /// Set random seed
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_rng_seed(
        IntPtr config, nuint value);

    /// <summary>
    /// Set stop token IDs
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_set_stop_token_ids(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPArray)] long[] token_ids,
        nuint token_ids_num);

    /// <summary>
    /// Validate generation config
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_generation_config_validate(IntPtr config);

    #endregion

    #region Decoded Results Methods

    /// <summary>
    /// Create decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_decoded_results_create([Out] out IntPtr results);

    /// <summary>
    /// Free decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_decoded_results_free(IntPtr results);

    /// <summary>
    /// Get string from decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_decoded_results_get_string(
        IntPtr results,
        IntPtr output,
        ref nuint output_size);

    /// <summary>
    /// Get performance metrics from decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_decoded_results_get_perf_metrics(
        IntPtr results,
        [Out] out IntPtr metrics);

    /// <summary>
    /// Free performance metrics
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_decoded_results_perf_metrics_free(IntPtr metrics);

    #endregion

    #region Performance Metrics Methods

    /// <summary>
    /// Get load time from performance metrics
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_load_time(
        IntPtr metrics, [Out] out float load_time);

    /// <summary>
    /// Get number of generated tokens
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_num_generation_tokens(
        IntPtr metrics, [Out] out nuint num_generation_tokens);

    /// <summary>
    /// Get number of input tokens
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_num_input_tokens(
        IntPtr metrics, [Out] out nuint num_input_tokens);

    /// <summary>
    /// Get time to first token
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_ttft(
        IntPtr metrics, [Out] out float mean, [Out] out float std);

    /// <summary>
    /// Get time per output token
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_tpot(
        IntPtr metrics, [Out] out float mean, [Out] out float std);

    /// <summary>
    /// Get throughput
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_perf_metrics_get_throughput(
        IntPtr metrics, [Out] out float mean, [Out] out float std);

    #endregion
}
