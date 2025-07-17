# GitHub Workflows

This directory contains GitHub Actions workflows for the OpenVINO.NET project, providing comprehensive CI/CD and performance testing capabilities.

## Workflows Overview

### 🔨 build-test.yml
**Purpose**: Continuous Integration for build and unit testing  
**Triggers**: Push to main, Pull Requests, Manual dispatch  
**Platforms**: Windows (win-x64), Linux (linux-x64)

**What it does:**
- Builds the entire solution on both Windows and Linux
- Downloads OpenVINO GenAI runtime (2025.2.0.0 stable)
- Runs unit tests with code coverage
- Uploads test results and build artifacts
- Publishes QuickDemo executables for both platforms

### 🧪 integration-test.yml
**Purpose**: Integration testing with real models and device testing  
**Triggers**: Manual dispatch, Weekly schedule (Sundays 2 AM UTC)  
**Platforms**: Windows (win-x64), Linux (linux-x64)

**Features:**
- Tests multiple devices (CPU, GPU, NPU) with automatic fallback
- Downloads and caches NPU-optimized model (`FluidInference/qwen3-0.6b-int4-ov-npu`)
- Runs actual inference tests to verify functionality
- Measures performance metrics (TPS, latency)
- Creates device compatibility reports
- Comments results on Pull Requests

**Manual Parameters:**
- `test_devices`: Comma-separated list of devices to test (default: CPU,GPU,NPU)
- `model_name`: HuggingFace model to use (default: FluidInference/qwen3-0.6b-int4-ov-npu)

### 📊 benchmark.yml
**Purpose**: Comprehensive performance benchmarking and comparison  
**Triggers**: Manual dispatch only  
**Platforms**: Windows (win-x64), Linux (linux-x64)

**Features:**
- Multi-iteration performance testing per device
- Memory usage monitoring and reporting
- Baseline performance comparison with historical data
- Detailed JSON metrics export for analysis
- Performance regression detection
- Platform-specific optimizations

**Manual Parameters:**
- `devices`: Devices to benchmark (default: CPU,GPU,NPU)
- `iterations`: Number of test iterations per device (default: 3)
- `model_name`: Model to use for benchmarking
- `compare_with_baseline`: Enable baseline comparison (default: true)

## Performance Metrics

All workflows collect and report the following metrics:

### Core Metrics
- **Tokens per Second (TPS)**: Primary throughput measure
- **First Token Latency**: Time to generate first token (ms)
- **Total Generation Time**: Complete inference duration
- **Memory Usage**: Peak memory consumption (MB)

### Device Coverage
- **CPU**: Always available, serves as baseline
- **GPU**: Tested when available (Intel/NVIDIA)
- **NPU**: Tested when available (Intel NPU)

### Model Configuration
- **Default Model**: `FluidInference/qwen3-0.6b-int4-ov-npu` (NPU-optimized, INT4 quantized)
- **Size**: ~1.2GB download
- **Cached**: Models are cached between workflow runs
- **Configurable**: Can specify different HuggingFace models

## Usage Examples

### Running Integration Tests
```bash
# Test all devices with default model
gh workflow run integration-test.yml

# Test specific devices
gh workflow run integration-test.yml -f test_devices="CPU,GPU"

# Use different model
gh workflow run integration-test.yml -f model_name="microsoft/DialoGPT-medium-ov"
```

### Running Performance Benchmarks
```bash
# Standard benchmark
gh workflow run benchmark.yml

# Extended benchmark with more iterations
gh workflow run benchmark.yml -f iterations=5

# NPU-only benchmark
gh workflow run benchmark.yml -f devices="NPU" -f iterations=10
```

### Viewing Results
- **Artifacts**: Download detailed results from workflow runs
- **PR Comments**: Integration and benchmark results automatically commented on PRs
- **Logs**: Real-time progress in workflow logs

## Expected Performance

### Typical Results (based on hardware)
- **CPU**: 12-15 TPS, 400-800ms first token
- **GPU**: 20-30 TPS, 300-600ms first token  
- **NPU**: 45-60 TPS, 200-400ms first token (INT4 model)

### Memory Usage
- **Model Loading**: ~1.5-2.0GB initial allocation
- **Inference**: +50-200MB per inference
- **Platform Differences**: Windows typically uses 10-15% more memory

## Troubleshooting

### Common Issues
1. **Model Download Failures**: Check network connectivity and HuggingFace availability
2. **Device Not Available**: Workflows gracefully handle missing devices (GPU/NPU)
3. **Memory Issues**: Large models may exceed runner memory limits
4. **Build Failures**: Usually indicate breaking changes in dependencies

### Debugging Steps
1. Check workflow logs for detailed error messages
2. Verify model availability on HuggingFace
3. Test locally with QuickDemo: `dotnet run --project samples/QuickDemo`
4. Enable verbose logging with `--verbosity detailed`

## Customization

### Adding New Devices
1. Update device lists in workflow files
2. Add device-specific setup steps if needed
3. Update expected performance baselines

### Custom Models
1. Ensure model is in OpenVINO format with required files:
   - `openvino_model.xml/bin`
   - `openvino_tokenizer.xml/bin`
   - `openvino_detokenizer.xml/bin`
   - `config.json`
   - `generation_config.json`

### Platform Support
To add new platforms:
1. Update matrix strategy in workflows
2. Add platform-specific OpenVINO runtime download steps
3. Update MSBuild targets for native library handling

## Contributing

When modifying workflows:
1. Test changes on fork first
2. Update this README with new features
3. Consider backward compatibility
4. Update expected performance baselines if needed