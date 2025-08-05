# GitHub Workflows

This directory contains GitHub Actions workflows for the OpenVINO.NET project, providing comprehensive CI/CD and performance testing capabilities.

## Workflows Overview

### 🔨 build-test.yml
**Purpose**: Continuous Integration for build and unit testing  
**Triggers**: Push to main, Pull Requests, Manual dispatch  
**Platforms**: Windows (win-x64), Linux (linux-x64)

**What it does:**
- Builds the entire solution on both Windows and Linux
- Downloads OpenVINO GenAI runtime (2025.3.0.0.dev20250801 nightly)
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

#### 1. **OpenVINO Runtime Download Failures**
**Symptoms**: 
- `gzip: stdin: not in gzip format`
- `tar: Child returned status 1`
- `Failed to download OpenVINO runtime from any URL`

**Solutions**:
- Workflows now include multiple URL fallbacks (ubuntu24, ubuntu22, different naming conventions)
- Check OpenVINO storage availability: https://storage.openvinotoolkit.org/repositories/openvino_genai/
- Verify internet connectivity and corporate firewall settings
- Manual verification: Try downloading URLs directly

**Workflow Improvements** (Applied):
- Multiple URL attempts with different Ubuntu versions and file formats
- File format validation before extraction
- Detailed error messages showing which URLs were tried
- Automatic directory structure detection

#### 2. **Model Download Failures**
**Symptoms**: Model files missing or corrupted from HuggingFace

**Solutions**:
- Check network connectivity and HuggingFace availability
- Verify model exists: https://huggingface.co/FluidInference/qwen3-0.6b-int4-ov-npu
- Models are cached between runs - clear cache if corrupted
- Use alternative models via workflow parameters

#### 3. **Device Not Available**
**Symptoms**: GPU/NPU tests fail with device errors

**Solutions**:
- Workflows gracefully handle missing devices with fallback to CPU
- Check GitHub runner capabilities (GPU support limited)
- NPU support only available on Intel-specific hardware
- Review device compatibility reports in workflow outputs

#### 4. **Memory Issues**
**Symptoms**: Out of memory errors during model loading or inference

**Solutions**:
- Large models may exceed runner memory limits (7GB on GitHub runners)
- Use smaller/quantized models for CI (like INT4 NPU model)
- Monitor memory usage with `--memory-monitoring` flag
- Consider self-hosted runners for larger models

#### 5. **Build Failures**
**Symptoms**: Compilation errors, missing dependencies

**Solutions**:
- Usually indicate breaking changes in dependencies
- Check MSBuild targets path resolution
- Verify .NET 8.0 SDK installation
- Review native library deployment

### Debugging Steps

#### For Download Issues:
1. **Check Workflow Logs**: Look for specific URL failures and error messages
2. **Manual URL Testing**: Try downloading URLs directly with curl/wget
3. **Verify OpenVINO Storage**: Check if files exist at storage.openvinotoolkit.org
4. **Test Local Setup**: Run download scripts locally to isolate the issue

#### For Runtime Issues:
1. **Check Device Logs**: Review device-specific error messages
2. **Verify Model Files**: Ensure all required OpenVINO files are present
3. **Test Locally**: Run QuickDemo locally: `dotnet run --project samples/QuickDemo`
4. **Enable Verbose Logging**: Use `--verbosity detailed` in dotnet commands

#### For Performance Issues:
1. **Review Metrics**: Check TPS and memory usage in workflow outputs
2. **Compare Baselines**: Look at historical performance trends
3. **Device Comparison**: Compare CPU vs GPU vs NPU performance
4. **Enable Memory Monitoring**: Use `--memory-monitoring` flag for detailed tracking

### URL Update Procedure

If OpenVINO download URLs change:

1. **Find New URLs**: Check OpenVINO releases and storage structure
2. **Update All Workflows**: Modify build-test.yml, integration-test.yml, benchmark.yml
3. **Test Changes**: Run workflows manually to verify downloads work
4. **Update Documentation**: Update this README with new URL patterns

**Current URL Pattern** (Fixed):
```bash
# Linux (try in order):
https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/2025.2/linux/openvino_genai_ubuntu24_2025.2.0.0_x86_64.tar.gz
https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/2025.2/linux/openvino_genai_ubuntu22_2025.2.0.0_x86_64.tar.gz
https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/2025.2/linux/openvino_genai_ubuntu20_2025.2.0.0_x86_64.tar.gz

# Windows (try in order):
https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/2025.2/windows/openvino_genai_windows_2025.2.0.0_x86_64.zip
https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/2025.2/windows/openvino_genai_runtime_windows_2025.2.0.0_x86_64.zip
```

**Important**: Note the path uses `2025.2` not `2025.2.0.0` - this was the key issue causing download failures.

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