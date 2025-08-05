# OpenVINO.NET.GenAI Tests

This directory contains unit and integration tests for the OpenVINO.NET.GenAI library.

## Test Categories

### Unit Tests
- **GenerationConfigTests** - Tests for generation configuration fluent API
- **ExceptionTests** - Tests for exception handling
- **WhisperPipelineTests** - Tests for Whisper configuration and audio utilities

### Integration Tests
- **IntegrationTests** - LLM pipeline tests that require the Qwen model
- **WhisperIntegrationTests** - Whisper pipeline tests that require the Whisper model

## Running Tests

### Prerequisites

1. **LLM Model**: Download the Qwen model for LLM tests
   - Run the QuickDemo first to download the model automatically
   - Or set `QUICKDEMO_MODEL_PATH` environment variable

2. **Whisper Model**: Download the Whisper model for speech recognition tests
   ```bash
   # Windows
   ./scripts/download-whisper-model.ps1
   
   # Linux/Mac
   ./scripts/download-whisper-model.sh
   ```
   - Or set `WHISPER_MODEL_PATH` environment variable

3. **Native Libraries**: Ensure OpenVINO runtime is available
   - Set `OPENVINO_RUNTIME_PATH` environment variable
   - Or run the download scripts in the scripts directory

### Running All Tests
```bash
dotnet test
```

### Running Specific Test Categories
```bash
# Unit tests only
dotnet test --filter "Category!=Integration"

# Integration tests only
dotnet test --filter "Category=Integration"

# Whisper tests only
dotnet test --filter "FullyQualifiedName~Whisper"
```

### Platform-Specific Notes

#### Windows
- Tests run on x64 architecture
- Native DLLs are automatically deployed via MSBuild targets

#### Linux
- Set `LD_LIBRARY_PATH` to the native library directory
- Example: `export LD_LIBRARY_PATH=/path/to/openvino/libs:$LD_LIBRARY_PATH`

#### macOS
- ARM64 (Apple Silicon) may require Rosetta for x64 binaries
- Set `DYLD_LIBRARY_PATH` similar to Linux if needed

## CI/CD Integration

The tests are automatically run in GitHub Actions on:
- Ubuntu (latest)
- Windows (latest)

Models are cached between runs to speed up the CI pipeline.

## Test Models

- **LLM Model**: `qwen3-0.6b-int4-ov` - Small quantized model for text generation
- **Whisper Model**: `FluidInference/whisper-tiny-int4-ov-npu` - Optimized for NPU but works on CPU

## Troubleshooting

### Tests are skipped
- Check that models are downloaded to the correct paths
- Verify environment variables are set correctly
- Check console output for specific skip reasons

### Native library errors
- Ensure OpenVINO runtime matches the version in the project
- Check that all dependent libraries are available
- On Linux/Mac, verify library paths are set correctly

### Performance issues
- First test run includes model loading which is slow
- Subsequent runs should be faster due to caching
- CPU tests are slower than GPU/NPU if available