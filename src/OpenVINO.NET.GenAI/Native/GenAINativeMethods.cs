using System.Runtime.InteropServices;

namespace OpenVINO.NET.GenAI.Native;

/// <summary>
/// P/Invoke declarations for OpenVINO GenAI C API
/// </summary>
internal static class GenAINativeMethods
{
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

    #region Whisper Pipeline Methods

    /// <summary>
    /// Create Whisper pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_whisper_pipeline_create(
        [MarshalAs(UnmanagedType.LPStr)] string models_path,
        [MarshalAs(UnmanagedType.LPStr)] string device,
        nuint property_args_size,
        [Out] out IntPtr pipe);

    /// <summary>
    /// Free Whisper pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_whisper_pipeline_free(IntPtr pipe);

    /// <summary>
    /// Generate transcription using Whisper pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_pipeline_generate(
        IntPtr pipe,
        [MarshalAs(UnmanagedType.LPArray)] float[] raw_speech_input,
        nuint raw_speech_input_size,
        IntPtr config,
        IntPtr streamer,
        [Out] out IntPtr results);

    /// <summary>
    /// Get generation config from Whisper pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_pipeline_get_generation_config(
        IntPtr pipe,
        [Out] out IntPtr config);

    /// <summary>
    /// Set generation config to Whisper pipeline
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_pipeline_set_generation_config(
        IntPtr pipe,
        IntPtr config);

    #endregion

    #region Whisper Generation Config Methods

    /// <summary>
    /// Create Whisper generation config
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_create([Out] out IntPtr config);

    /// <summary>
    /// Free Whisper generation config
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_whisper_generation_config_free(IntPtr config);

    /// <summary>
    /// Set language for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_language(
        IntPtr config, [MarshalAs(UnmanagedType.LPStr)] string language);

    /// <summary>
    /// Set task for Whisper generation (transcribe/translate)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_task(
        IntPtr config, [MarshalAs(UnmanagedType.LPStr)] string task);

    /// <summary>
    /// Set return_timestamps flag for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_return_timestamps(
        IntPtr config, [MarshalAs(UnmanagedType.I1)] bool return_timestamps);

    /// <summary>
    /// Set initial prompt for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_initial_prompt(
        IntPtr config, [MarshalAs(UnmanagedType.LPStr)] string initial_prompt);

    /// <summary>
    /// Set hotwords for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_hotwords(
        IntPtr config, [MarshalAs(UnmanagedType.LPStr)] string hotwords);

    /// <summary>
    /// Set max_initial_timestamp_index for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_max_initial_timestamp_index(
        IntPtr config, nuint max_initial_timestamp_index);

    /// <summary>
    /// Set decoder_start_token_id for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_decoder_start_token_id(
        IntPtr config, long decoder_start_token_id);

    /// <summary>
    /// Set suppress_tokens for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_suppress_tokens(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPArray)] long[] suppress_tokens,
        nuint suppress_tokens_size);

    /// <summary>
    /// Set begin_suppress_tokens for Whisper generation
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_generation_config_set_begin_suppress_tokens(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPArray)] long[] begin_suppress_tokens,
        nuint begin_suppress_tokens_size);

    #endregion

    #region Whisper Decoded Results Methods

    /// <summary>
    /// Create Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_create([Out] out IntPtr results);

    /// <summary>
    /// Free Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_whisper_decoded_results_free(IntPtr results);

    /// <summary>
    /// Get number of text results from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_texts_size(
        IntPtr results,
        [Out] out nuint texts_size);

    /// <summary>
    /// Get text at index from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_text_at(
        IntPtr results,
        nuint index,
        IntPtr output,
        ref nuint output_size);

    /// <summary>
    /// Get number of chunks from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_chunks_size(
        IntPtr results,
        [Out] out nuint chunks_size);

    /// <summary>
    /// Get chunk at index from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_chunk_at(
        IntPtr results,
        nuint index,
        [Out] out whisper_result_chunk chunk);

    /// <summary>
    /// Get scores from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_scores(
        IntPtr results,
        [MarshalAs(UnmanagedType.LPArray)] float[] scores,
        nuint scores_size);

    /// <summary>
    /// Get performance metrics from Whisper decoded results
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_decoded_results_get_perf_metrics(
        IntPtr results,
        [Out] out IntPtr metrics);

    #endregion

    #region Whisper Performance Metrics Methods

    /// <summary>
    /// Get features extraction duration from Whisper performance metrics
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ov_status_e ov_genai_whisper_perf_metrics_get_features_extraction_duration(
        IntPtr metrics, [Out] out float mean, [Out] out float std);

    /// <summary>
    /// Free Whisper performance metrics
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ov_genai_whisper_perf_metrics_free(IntPtr metrics);

    #endregion
}