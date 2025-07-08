#pragma once

#ifdef _WIN32
    #ifdef WHISPER_WRAPPER_EXPORTS
        #define WHISPER_WRAPPER_API extern "C" __declspec(dllexport)
    #else
        #define WHISPER_WRAPPER_API extern "C" __declspec(dllimport)
    #endif
#else
    #define WHISPER_WRAPPER_API extern "C" __attribute__((visibility("default")))
#endif

#include <cstdint>
#include <cstddef>

// Status codes matching the ones used in our C# code
typedef enum {
    OV_STATUS_OK = 0,
    OV_STATUS_GENERAL_ERROR = -1,
    OV_STATUS_NOT_IMPLEMENTED = -2,
    OV_STATUS_NETWORK_NOT_LOADED = -3,
    OV_STATUS_PARAMETER_MISMATCH = -4,
    OV_STATUS_NOT_FOUND = -5,
    OV_STATUS_OUT_OF_BOUNDS = -6,
    OV_STATUS_UNEXPECTED = -7,
    OV_STATUS_REQUEST_BUSY = -8,
    OV_STATUS_RESULT_NOT_READY = -9,
    OV_STATUS_NOT_ALLOCATED = -10,
    OV_STATUS_INFER_NOT_STARTED = -11,
    OV_STATUS_NETWORK_NOT_READ = -12,
    OV_STATUS_INFER_CANCELLED = -13,
    OV_STATUS_INVALID_C_PARAM = -14,
    OV_STATUS_UNKNOWN_C_ERROR = -15,
    OV_STATUS_NOT_IMPLEMENT_C_METHOD = -16,
    OV_STATUS_UNKNOW_EXCEPTION = -17
} ov_status_e;

// Whisper result chunk structure
typedef struct {
    float start_time;
    float end_time;
    const char* text;
} whisper_result_chunk;

// Pipeline functions
WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_pipeline_create(
    const char* models_path,
    const char* device,
    size_t property_args_size,
    void** pipe);

WHISPER_WRAPPER_API void ov_genai_whisper_pipeline_free(void* pipe);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_pipeline_generate(
    void* pipe,
    const float* raw_speech_input,
    size_t raw_speech_input_size,
    void* config,
    void** results);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_pipeline_get_generation_config(
    void* pipe,
    void** config);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_pipeline_set_generation_config(
    void* pipe,
    void* config);

// Generation config functions
WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_create(void** config);

WHISPER_WRAPPER_API void ov_genai_whisper_generation_config_free(void* config);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_language(
    void* config, const char* language);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_task(
    void* config, const char* task);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_return_timestamps(
    void* config, bool return_timestamps);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_initial_prompt(
    void* config, const char* initial_prompt);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_hotwords(
    void* config, const char* hotwords);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_max_initial_timestamp_index(
    void* config, size_t max_initial_timestamp_index);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_decoder_start_token_id(
    void* config, int64_t decoder_start_token_id);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_suppress_tokens(
    void* config,
    const int64_t* suppress_tokens,
    size_t suppress_tokens_size);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_generation_config_set_begin_suppress_tokens(
    void* config,
    const int64_t* begin_suppress_tokens,
    size_t begin_suppress_tokens_size);

// Results functions
WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_create(void** results);

WHISPER_WRAPPER_API void ov_genai_whisper_decoded_results_free(void* results);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_texts_size(
    void* results,
    size_t* texts_size);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_text_at(
    void* results,
    size_t index,
    void* output,
    size_t output_size);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_chunks_size(
    void* results,
    size_t* chunks_size);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_chunk_at(
    void* results,
    size_t index,
    whisper_result_chunk* chunk);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_scores(
    void* results,
    float* scores,
    size_t scores_size);

WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_decoded_results_get_perf_metrics(
    void* results,
    void** metrics);

// Performance metrics functions
WHISPER_WRAPPER_API ov_status_e ov_genai_whisper_perf_metrics_get_features_extraction_duration(
    void* metrics, float* mean, float* std);

WHISPER_WRAPPER_API void ov_genai_whisper_perf_metrics_free(void* metrics);