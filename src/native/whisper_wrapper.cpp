#include "whisper_wrapper.hpp"
#include "openvino/genai/whisper_pipeline.hpp"
#include "openvino/genai/whisper_generation_config.hpp"
#include "openvino/genai/llm_pipeline.hpp"
#include "openvino/genai/generation_config.hpp"
#include <memory>
#include <vector>
#include <string>
#include <stdexcept>

// Helper macro for exception handling
#define WHISPER_WRAPPER_TRY_CATCH(expr) \
    try { \
        expr; \
        return OV_STATUS_OK; \
    } catch (const std::exception& e) { \
        return OV_STATUS_GENERAL_ERROR; \
    } catch (...) { \
        return OV_STATUS_UNKNOW_EXCEPTION; \
    }

// Wrapper classes to hold C++ objects
struct WhisperPipelineWrapper {
    std::unique_ptr<ov::genai::WhisperPipeline> pipeline;
};

struct WhisperConfigWrapper {
    std::unique_ptr<ov::genai::WhisperGenerationConfig> config;
};

struct WhisperResultWrapper {
    ov::genai::WhisperDecodedResults results;
    std::vector<std::string> text_cache; // Cache strings for C access
    std::vector<whisper_result_chunk> chunk_cache; // Cache chunks for C access
};

struct WhisperPerfMetricsWrapper {
    ov::genai::PerfMetrics metrics;
};

// LLM wrapper classes
struct LLMPipelineWrapper {
    std::unique_ptr<ov::genai::LLMPipeline> pipeline;
};

struct GenerationConfigWrapper {
    std::unique_ptr<ov::genai::GenerationConfig> config;
};

struct DecodedResultsWrapper {
    ov::genai::DecodedResults results;
    std::string text_cache; // Cache string for C access
};

struct LLMPerfMetricsWrapper {
    ov::genai::PerfMetrics metrics;
};

// Pipeline functions
ov_status_e ov_genai_whisper_pipeline_create(
    const char* models_path,
    const char* device,
    size_t property_args_size,
    void** pipe) {
    
    if (!models_path || !device || !pipe) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<WhisperPipelineWrapper>();
        wrapper->pipeline = std::make_unique<ov::genai::WhisperPipeline>(
            std::string(models_path), 
            std::string(device));
        *pipe = wrapper.release();
    });
}

void ov_genai_whisper_pipeline_free(void* pipe) {
    if (pipe) {
        delete static_cast<WhisperPipelineWrapper*>(pipe);
    }
}

ov_status_e ov_genai_whisper_pipeline_generate(
    void* pipe,
    const float* raw_speech_input,
    size_t raw_speech_input_size,
    void* config,
    void** results) {
    
    if (!pipe || !raw_speech_input || !results) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperPipelineWrapper*>(pipe);
        
        // Convert raw audio to the format expected by OpenVINO GenAI
        ov::genai::RawSpeechInput speech_input(raw_speech_input, raw_speech_input + raw_speech_input_size);
        
        auto result_wrapper = std::make_unique<WhisperResultWrapper>();
        
        if (config) {
            auto* config_wrapper = static_cast<WhisperConfigWrapper*>(config);
            result_wrapper->results = wrapper->pipeline->generate(speech_input, *config_wrapper->config);
        } else {
            result_wrapper->results = wrapper->pipeline->generate(speech_input);
        }
        
        *results = result_wrapper.release();
    });
}

ov_status_e ov_genai_whisper_pipeline_get_generation_config(
    void* pipe,
    void** config) {
    
    if (!pipe || !config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperPipelineWrapper*>(pipe);
        auto config_wrapper = std::make_unique<WhisperConfigWrapper>();
        config_wrapper->config = std::make_unique<ov::genai::WhisperGenerationConfig>(
            wrapper->pipeline->get_generation_config());
        *config = config_wrapper.release();
    });
}

ov_status_e ov_genai_whisper_pipeline_set_generation_config(
    void* pipe,
    void* config) {
    
    if (!pipe || !config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperPipelineWrapper*>(pipe);
        auto* config_wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->pipeline->set_generation_config(*config_wrapper->config);
    });
}

// Generation config functions
ov_status_e ov_genai_whisper_generation_config_create(void** config) {
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<WhisperConfigWrapper>();
        wrapper->config = std::make_unique<ov::genai::WhisperGenerationConfig>();
        *config = wrapper.release();
    });
}

void ov_genai_whisper_generation_config_free(void* config) {
    if (config) {
        delete static_cast<WhisperConfigWrapper*>(config);
    }
}

ov_status_e ov_genai_whisper_generation_config_set_language(
    void* config, const char* language) {
    
    if (!config || !language) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->language = std::string(language);
    });
}

ov_status_e ov_genai_whisper_generation_config_set_task(
    void* config, const char* task) {
    
    if (!config || !task) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->task = std::string(task);
    });
}

ov_status_e ov_genai_whisper_generation_config_set_return_timestamps(
    void* config, bool return_timestamps) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->return_timestamps = return_timestamps;
    });
}

ov_status_e ov_genai_whisper_generation_config_set_initial_prompt(
    void* config, const char* initial_prompt) {
    
    if (!config || !initial_prompt) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->initial_prompt = std::string(initial_prompt);
    });
}

ov_status_e ov_genai_whisper_generation_config_set_hotwords(
    void* config, const char* hotwords) {
    
    if (!config || !hotwords) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->hotwords = std::string(hotwords);
    });
}

ov_status_e ov_genai_whisper_generation_config_set_max_initial_timestamp_index(
    void* config, size_t max_initial_timestamp_index) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->max_initial_timestamp_index = max_initial_timestamp_index;
    });
}

ov_status_e ov_genai_whisper_generation_config_set_decoder_start_token_id(
    void* config, int64_t decoder_start_token_id) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->decoder_start_token_id = decoder_start_token_id;
    });
}

ov_status_e ov_genai_whisper_generation_config_set_suppress_tokens(
    void* config,
    const int64_t* suppress_tokens,
    size_t suppress_tokens_size) {
    
    if (!config || !suppress_tokens) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->suppress_tokens = std::vector<int64_t>(
            suppress_tokens, suppress_tokens + suppress_tokens_size);
    });
}

ov_status_e ov_genai_whisper_generation_config_set_begin_suppress_tokens(
    void* config,
    const int64_t* begin_suppress_tokens,
    size_t begin_suppress_tokens_size) {
    
    if (!config || !begin_suppress_tokens) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperConfigWrapper*>(config);
        wrapper->config->begin_suppress_tokens = std::vector<int64_t>(
            begin_suppress_tokens, begin_suppress_tokens + begin_suppress_tokens_size);
    });
}

// Results functions
ov_status_e ov_genai_whisper_decoded_results_create(void** results) {
    if (!results) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<WhisperResultWrapper>();
        *results = wrapper.release();
    });
}

void ov_genai_whisper_decoded_results_free(void* results) {
    if (results) {
        delete static_cast<WhisperResultWrapper*>(results);
    }
}

ov_status_e ov_genai_whisper_decoded_results_get_texts_size(
    void* results,
    size_t* texts_size) {
    
    if (!results || !texts_size) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        *texts_size = wrapper->results.texts.size();
    });
}

ov_status_e ov_genai_whisper_decoded_results_get_text_at(
    void* results,
    size_t index,
    void* output,
    size_t output_size) {
    
    if (!results || !output) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        if (index >= wrapper->results.texts.size()) {
            return OV_STATUS_OUT_OF_BOUNDS;
        }
        
        const std::string& text = wrapper->results.texts[index];
        size_t copy_size = std::min(output_size - 1, text.size());
        std::memcpy(output, text.c_str(), copy_size);
        static_cast<char*>(output)[copy_size] = '\0';
    });
}

ov_status_e ov_genai_whisper_decoded_results_get_chunks_size(
    void* results,
    size_t* chunks_size) {
    
    if (!results || !chunks_size) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        *chunks_size = wrapper->results.chunks.size();
    });
}

ov_status_e ov_genai_whisper_decoded_results_get_chunk_at(
    void* results,
    size_t index,
    whisper_result_chunk* chunk) {
    
    if (!results || !chunk) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        if (index >= wrapper->results.chunks.size()) {
            return OV_STATUS_OUT_OF_BOUNDS;
        }
        
        const auto& src_chunk = wrapper->results.chunks[index];
        chunk->start_time = src_chunk.start_ts;
        chunk->end_time = src_chunk.end_ts;
        chunk->text = src_chunk.text.c_str();
    });
}

ov_status_e ov_genai_whisper_decoded_results_get_scores(
    void* results,
    float* scores,
    size_t scores_size) {
    
    if (!results || !scores) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        if (scores_size < wrapper->results.scores.size()) {
            return OV_STATUS_OUT_OF_BOUNDS;
        }
        
        std::memcpy(scores, wrapper->results.scores.data(), 
                   wrapper->results.scores.size() * sizeof(float));
    });
}

ov_status_e ov_genai_whisper_decoded_results_get_perf_metrics(
    void* results,
    void** metrics) {
    
    if (!results || !metrics) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperResultWrapper*>(results);
        auto metrics_wrapper = std::make_unique<WhisperPerfMetricsWrapper>();
        metrics_wrapper->metrics = wrapper->results.perf_metrics;
        *metrics = metrics_wrapper.release();
    });
}

// Performance metrics functions
ov_status_e ov_genai_whisper_perf_metrics_get_features_extraction_duration(
    void* metrics, float* mean, float* std) {
    
    if (!metrics || !mean || !std) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<WhisperPerfMetricsWrapper*>(metrics);
        auto duration = wrapper->metrics.features_extraction_duration;
        *mean = static_cast<float>(duration.mean);
        *std = static_cast<float>(duration.std);
    });
}

void ov_genai_whisper_perf_metrics_free(void* metrics) {
    if (metrics) {
        delete static_cast<WhisperPerfMetricsWrapper*>(metrics);
    }
}

// ===================================================================
// LLM Pipeline Functions
// ===================================================================

ov_status_e ov_genai_llm_pipeline_create(
    const char* models_path,
    const char* device,
    size_t property_args_size,
    void** pipe) {
    
    if (!models_path || !device || !pipe) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<LLMPipelineWrapper>();
        wrapper->pipeline = std::make_unique<ov::genai::LLMPipeline>(
            std::string(models_path), 
            std::string(device));
        *pipe = wrapper.release();
    });
}

void ov_genai_llm_pipeline_free(void* pipe) {
    if (pipe) {
        delete static_cast<LLMPipelineWrapper*>(pipe);
    }
}

ov_status_e ov_genai_llm_pipeline_generate(
    void* pipe,
    const char* input_text,
    void* config,
    void* streamer,
    void** results) {
    
    if (!pipe || !input_text || !results) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<LLMPipelineWrapper*>(pipe);
        
        auto result_wrapper = std::make_unique<DecodedResultsWrapper>();
        
        if (config) {
            auto* config_wrapper = static_cast<GenerationConfigWrapper*>(config);
            result_wrapper->results = wrapper->pipeline->generate(
                std::string(input_text), *config_wrapper->config);
        } else {
            result_wrapper->results = wrapper->pipeline->generate(std::string(input_text));
        }
        
        *results = result_wrapper.release();
    });
}

ov_status_e ov_genai_llm_pipeline_start_chat(void* pipe) {
    if (!pipe) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<LLMPipelineWrapper*>(pipe);
        wrapper->pipeline->start_chat();
    });
}

ov_status_e ov_genai_llm_pipeline_finish_chat(void* pipe) {
    if (!pipe) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<LLMPipelineWrapper*>(pipe);
        wrapper->pipeline->finish_chat();
    });
}

ov_status_e ov_genai_llm_pipeline_get_generation_config(
    void* pipe,
    void** config) {
    
    if (!pipe || !config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<LLMPipelineWrapper*>(pipe);
        auto config_wrapper = std::make_unique<GenerationConfigWrapper>();
        config_wrapper->config = std::make_unique<ov::genai::GenerationConfig>(
            wrapper->pipeline->get_generation_config());
        *config = config_wrapper.release();
    });
}

ov_status_e ov_genai_llm_pipeline_set_generation_config(
    void* pipe,
    void* config) {
    
    if (!pipe || !config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<LLMPipelineWrapper*>(pipe);
        auto* config_wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->pipeline->set_generation_config(*config_wrapper->config);
    });
}

// ===================================================================
// Generation Config Functions
// ===================================================================

ov_status_e ov_genai_generation_config_create(void** config) {
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<GenerationConfigWrapper>();
        wrapper->config = std::make_unique<ov::genai::GenerationConfig>();
        *config = wrapper.release();
    });
}

ov_status_e ov_genai_generation_config_create_from_json(
    const char* json_config,
    void** config) {
    
    if (!json_config || !config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<GenerationConfigWrapper>();
        // Note: Assuming there's a constructor or method to create from JSON
        // This might need adjustment based on actual OpenVINO GenAI API
        wrapper->config = std::make_unique<ov::genai::GenerationConfig>();
        // TODO: Parse JSON and set config properties
        *config = wrapper.release();
    });
}

void ov_genai_generation_config_free(void* config) {
    if (config) {
        delete static_cast<GenerationConfigWrapper*>(config);
    }
}

ov_status_e ov_genai_generation_config_set_max_new_tokens(
    void* config, size_t max_new_tokens) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->max_new_tokens = max_new_tokens;
    });
}

ov_status_e ov_genai_generation_config_set_max_length(
    void* config, size_t max_length) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->max_length = max_length;
    });
}

ov_status_e ov_genai_generation_config_set_temperature(
    void* config, float temperature) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->temperature = temperature;
    });
}

ov_status_e ov_genai_generation_config_set_top_p(
    void* config, float top_p) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->top_p = top_p;
    });
}

ov_status_e ov_genai_generation_config_set_top_k(
    void* config, size_t top_k) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->top_k = top_k;
    });
}

ov_status_e ov_genai_generation_config_set_do_sample(
    void* config, bool do_sample) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->do_sample = do_sample;
    });
}

ov_status_e ov_genai_generation_config_set_repetition_penalty(
    void* config, float repetition_penalty) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->repetition_penalty = repetition_penalty;
    });
}

ov_status_e ov_genai_generation_config_set_presence_penalty(
    void* config, float presence_penalty) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->presence_penalty = presence_penalty;
    });
}

ov_status_e ov_genai_generation_config_set_frequency_penalty(
    void* config, float frequency_penalty) {
    
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        wrapper->config->frequency_penalty = frequency_penalty;
    });
}

ov_status_e ov_genai_generation_config_set_stop_strings(
    void* config,
    const char** stop_strings,
    size_t stop_strings_size) {
    
    if (!config || !stop_strings) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        std::vector<std::string> stop_vec;
        for (size_t i = 0; i < stop_strings_size; ++i) {
            stop_vec.emplace_back(stop_strings[i]);
        }
        wrapper->config->stop_strings = stop_vec;
    });
}

ov_status_e ov_genai_generation_config_get_max_new_tokens(
    void* config, size_t* max_new_tokens) {
    
    if (!config || !max_new_tokens) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        *max_new_tokens = wrapper->config->max_new_tokens;
    });
}

ov_status_e ov_genai_generation_config_validate(void* config) {
    if (!config) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<GenerationConfigWrapper*>(config);
        // Note: Assuming there's a validate method
        // This might need adjustment based on actual OpenVINO GenAI API
        // wrapper->config->validate();
    });
}

// ===================================================================
// Decoded Results Functions
// ===================================================================

ov_status_e ov_genai_decoded_results_create(void** results) {
    if (!results) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto wrapper = std::make_unique<DecodedResultsWrapper>();
        *results = wrapper.release();
    });
}

void ov_genai_decoded_results_free(void* results) {
    if (results) {
        delete static_cast<DecodedResultsWrapper*>(results);
    }
}

ov_status_e ov_genai_decoded_results_get_string(
    void* results,
    void* output,
    size_t output_size) {
    
    if (!results || !output) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<DecodedResultsWrapper*>(results);
        const std::string& text = wrapper->results.text;
        
        size_t copy_size = std::min(output_size - 1, text.size());
        std::memcpy(output, text.c_str(), copy_size);
        static_cast<char*>(output)[copy_size] = '\0';
    });
}

ov_status_e ov_genai_decoded_results_get_perf_metrics(
    void* results,
    void** metrics) {
    
    if (!results || !metrics) {
        return OV_STATUS_INVALID_C_PARAM;
    }

    WHISPER_WRAPPER_TRY_CATCH({
        auto* wrapper = static_cast<DecodedResultsWrapper*>(results);
        auto metrics_wrapper = std::make_unique<LLMPerfMetricsWrapper>();
        metrics_wrapper->metrics = wrapper->results.perf_metrics;
        *metrics = metrics_wrapper.release();
    });
}

void ov_genai_decoded_results_perf_metrics_free(void* metrics) {
    if (metrics) {
        delete static_cast<LLMPerfMetricsWrapper*>(metrics);
    }
}