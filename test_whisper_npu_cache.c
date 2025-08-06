#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "openvino/genai/c/whisper_pipeline.h"
#include "whisper_utils.h"

int main(int argc, char* argv[]) {
    if (argc != 3 && argc != 4) {
        fprintf(stderr, "Usage: %s <MODEL_DIR> \"<WAV_FILE_PATH>\" [DEVICE]\n", argv[0]);
        return EXIT_FAILURE;
    }

    const char* model_path = argv[1];
    const char* wav_file_path = argv[2];
    const char* device = (argc == 4) ? argv[3] : "CPU";

    int exit_code = EXIT_SUCCESS;
    ov_genai_whisper_pipeline* pipeline = NULL;
    ov_genai_whisper_generation_config* config = NULL;
    ov_genai_whisper_decoded_results* results = NULL;
    float* audio_data = NULL;
    size_t audio_length = 0;

    float file_sample_rate;
    if (load_wav_file(wav_file_path, &audio_data, &audio_length, &file_sample_rate) != 0) {
        exit_code = EXIT_FAILURE;
        goto err;
    }

    // Resample if needed
    if (file_sample_rate != 16000.0f) {
        size_t resampled_length;
        float* resampled_audio = resample_audio(audio_data, audio_length, file_sample_rate, 16000.0f, &resampled_length);
        if (!resampled_audio) {
            fprintf(stderr, "Error: Failed to resample audio\n");
            exit_code = EXIT_FAILURE;
            goto err;
        }
        free(audio_data);
        audio_data = resampled_audio;
        audio_length = resampled_length;
    }

    printf("Creating pipeline with CACHE_DIR for NPU...\n");
    
    // Create pipeline with CACHE_DIR property for NPU
    ov_status_e status;
    if (strcmp(device, "NPU") == 0) {
        // Pass CACHE_DIR property for NPU
        status = ov_genai_whisper_pipeline_create(model_path, device, 2, &pipeline, 
                                                  "CACHE_DIR", "C:\\Users\\brand\\code\\OpenVINO.GenAI.NET\\npu_cache");
    } else {
        // No additional properties for other devices
        status = ov_genai_whisper_pipeline_create(model_path, device, 0, &pipeline);
    }
    
    if (status != OK) {
        fprintf(stderr, "Error: Failed to create pipeline (status: %d)\n", status);
        exit_code = EXIT_FAILURE;
        goto err;
    }
    
    printf("Pipeline created successfully.\n");
    printf("Generating transcription...\n");

    status = ov_genai_whisper_generation_config_create(&config);
    if (status != OK) {
        fprintf(stderr, "Error: Failed to create config (status: %d)\n", status);
        exit_code = EXIT_FAILURE;
        goto err;
    }

    status = ov_genai_whisper_generation_config_set_task(config, "transcribe");
    if (status != OK) {
        fprintf(stderr, "Error: Failed to set task (status: %d)\n", status);
        exit_code = EXIT_FAILURE;
        goto err;
    }

    status = ov_genai_whisper_pipeline_generate(pipeline, audio_data, audio_length, config, &results);
    if (status != OK) {
        fprintf(stderr, "Error: Failed to generate (status: %d)\n", status);
        exit_code = EXIT_FAILURE;
        goto err;
    }

    size_t output_size = 0;
    status = ov_genai_whisper_decoded_results_get_string(results, NULL, &output_size);
    if (status != OK) {
        fprintf(stderr, "Error: Failed to get output size (status: %d)\n", status);
        exit_code = EXIT_FAILURE;
        goto err;
    }

    char* output = (char*)malloc(output_size);
    if (!output) {
        fprintf(stderr, "Error: Failed to allocate memory for output\n");
        exit_code = EXIT_FAILURE;
        goto err;
    }

    status = ov_genai_whisper_decoded_results_get_string(results, output, &output_size);
    if (status != OK) {
        fprintf(stderr, "Error: Failed to get output (status: %d)\n", status);
        free(output);
        exit_code = EXIT_FAILURE;
        goto err;
    }

    printf("Transcription: %s\n", output);
    free(output);

err:
    if (pipeline) ov_genai_whisper_pipeline_free(pipeline);
    if (config) ov_genai_whisper_generation_config_free(config);
    if (results) ov_genai_whisper_decoded_results_free(results);
    if (audio_data) free(audio_data);

    return exit_code;
}