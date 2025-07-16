# Troubleshooting Guide

## Common Issues

### 1. "OpenVINO runtime not found"
```
Error: The specified module could not be found. (Exception from HRESULT: 0x8007007E)
```
**Solution**: Ensure OpenVINO GenAI runtime DLLs are in your PATH or application directory.

**Steps to fix:**
1. Download OpenVINO GenAI runtime from the official repository
2. Extract to a directory and add to your system PATH
3. Or copy DLLs to your application's output directory
4. Restart your application

### 2. "Device not supported"
```
Error: Failed to create LLM pipeline on GPU: Device GPU is not supported
```
**Solutions**:
- Check device availability: `dotnet run --project samples/QuickDemo -- --benchmark`
- Use CPU fallback: `dotnet run --project samples/QuickDemo -- --device CPU`
- Install appropriate drivers (Intel GPU driver for GPU support, Intel NPU driver for NPU)

### 3. "Model download fails"
```
Error: Failed to download model files from HuggingFace
```
**Solutions**:
- Check internet connectivity
- Verify HuggingFace is accessible
- Manually download model files to `./Models/Qwen3-0.6B-fp16-ov/`

### 4. "Out of memory during inference"
```
Error: Insufficient memory to load model
```
**Solutions**:
- Use a smaller model
- Reduce max_tokens parameter
- Close other memory-intensive applications
- Consider using INT4 quantized models

### 5. "Build errors"
```
Error: Could not load file or assembly 'OpenVINO.NET.Core'
```
**Solutions**:
- Ensure all prerequisites are installed
- Clean and rebuild: `dotnet clean && dotnet build`
- Check .NET version: `dotnet --version`
- Verify OpenVINO runtime is properly installed

## Debug Mode

Enable detailed logging by setting environment variable:

### Windows
```cmd
set OPENVINO_LOG_LEVEL=DEBUG
```

### Linux/macOS
```bash
export OPENVINO_LOG_LEVEL=DEBUG
```

## Diagnostic Commands

### Check Installation
```bash
# Verify .NET version
dotnet --version

# Test basic functionality
dotnet run --project samples/QuickDemo

# Check all available devices
dotnet run --project samples/QuickDemo -- --benchmark
```

### Environment Verification
```bash
# Check if OpenVINO runtime is in PATH
where openvino_genai_c.dll  # Windows
which openvino_genai_c.so   # Linux

# List loaded modules (Windows)
dumpbin /imports your_app.exe
```

## Performance Issues

### Slow Inference
- Check device selection (GPU usually faster than CPU)
- Verify model is optimized for your hardware
- Monitor system resources
- Consider using quantized models

### High Memory Usage
- Use streaming generation for long texts
- Reduce batch size if using batch processing
- Consider smaller models
- Monitor memory usage during inference

## Getting Help

### Before Reporting Issues
1. Check this troubleshooting guide
2. Verify your environment meets the requirements
3. Test with the QuickDemo sample
4. Enable debug logging
5. Check device availability

### Information to Include
When reporting issues, please include:
- Operating system and version
- .NET version (`dotnet --version`)
- OpenVINO GenAI runtime version
- Full error message and stack trace
- Steps to reproduce the issue
- Output of benchmark command
- Debug logs if available

### Community Support
- GitHub Issues: Report bugs and feature requests
- Discord: Join our community for real-time help
- Documentation: Check the official OpenVINO documentation

## Advanced Troubleshooting

### Native Library Issues
1. **Verify DLL dependencies**: Use tools like Dependency Walker to check missing dependencies
2. **Check architecture**: Ensure you're using the correct x64 libraries
3. **Verify OpenVINO installation**: Test with OpenVINO samples directly

### Model Loading Issues
1. **Check model format**: Ensure model is in OpenVINO format (.xml/.bin)
2. **Verify model path**: Use absolute paths to eliminate path issues
3. **Check permissions**: Ensure read access to model files

### Device Selection Problems
1. **List available devices**: Use the benchmark command
2. **Check drivers**: Update GPU/NPU drivers
3. **Test device directly**: Use OpenVINO samples to verify device functionality
