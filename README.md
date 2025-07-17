# OpenVINO.NET

A comprehensive C# wrapper for OpenVINO and OpenVINO GenAI, providing idiomatic .NET APIs for AI inference and generative AI tasks.

**This is very much so in progress. Please do not use yet :). If this is something that might be helpful, leave an issue and let us know your usecase and we can try to incorporate as needed or else the main focus will be on Windows .NET apps **

## Features

- **OpenVINO.NET.Core**: Core OpenVINO functionality for model inference
- **OpenVINO.NET.GenAI**: Generative AI capabilities including LLM pipelines
- **OpenVINO.NET.Native**: Native library management and deployment
- **Modern C# API**: Async/await, IAsyncEnumerable, SafeHandle resource management
- **Windows x64 Support**: Optimized for Windows deployment scenarios

## Requirements

- .NET 9.0 or later
- Windows x64
- OpenVINO GenAI 2025.2.0.0 runtime

## Quick Start

### Option 1: Quick Demo (Recommended)

The easiest way to get started is with the QuickDemo application that automatically downloads a model:

```bash
# Run with default CPU device
dotnet run --project samples/QuickDemo

# Run on specific device
dotnet run --project samples/QuickDemo -- --device GPU

# Compare performance across all devices
dotnet run --project samples/QuickDemo -- --benchmark
```

**Sample Output:**
```
OpenVINO.NET Quick Demo
=======================
Model: Qwen3-0.6B-fp16-ov
Temperature: 0.7, Max Tokens: 100

✓ Model found at: ./Models/Qwen3-0.6B-fp16-ov
Device: CPU

Prompt 1: "Explain quantum computing in simple terms:"
Response: "Quantum computing is a revolutionary technology that uses quantum mechanics principles..."
Performance: 12.4 tokens/sec, First token: 450ms
```

### Option 2: Code Integration

For integrating into your own applications:

```csharp
using OpenVINO.NET.GenAI;

using var pipeline = new LLMPipeline("path/to/model", "CPU");
var config = GenerationConfig.Default.WithMaxTokens(100).WithTemperature(0.7f);

string result = await pipeline.GenerateAsync("Hello, world!", config);
Console.WriteLine(result);
```

### Streaming Generation

```csharp
using OpenVINO.NET.GenAI;

using var pipeline = new LLMPipeline("path/to/model", "CPU");
var config = GenerationConfig.Default.WithMaxTokens(100);

await foreach (var token in pipeline.GenerateStreamAsync("Tell me a story", config))
{
    Console.Write(token);
}
```

## Projects

- `OpenVINO.NET.Core` - Core OpenVINO wrapper
- `OpenVINO.NET.GenAI` - GenAI functionality
- `OpenVINO.NET.Native` - Native library management
- `QuickDemo` - **Quick start demo with automatic model download**
- `TextGeneration.Sample` - Basic text generation example
- `StreamingChat.Sample` - Streaming chat application

## Architecture

### Three-Layer Design

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Application                         │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│                OpenVINO.NET.GenAI                          │
│  • LLMPipeline (High-level API)                            │
│  • GenerationConfig (Fluent configuration)                 │
│  • ChatSession (Conversation management)                   │
│  • IAsyncEnumerable streaming                              │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│                OpenVINO.NET.Core                           │
│  • Core OpenVINO functionality                             │
│  • Model loading and inference                             │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│               OpenVINO.NET.Native                          │
│  • P/Invoke declarations                                    │
│  • SafeHandle resource management                          │
│  • MSBuild targets for DLL deployment                      │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│            OpenVINO GenAI C API                            │
│  • Native OpenVINO GenAI runtime                           │
│  • Version: 2025.2.0.0                                     │
└─────────────────────────────────────────────────────────────┘
```

### Key Features

- **Memory Safe**: SafeHandle pattern for automatic resource cleanup
- **Async/Await**: Full async support with cancellation tokens
- **Streaming**: Real-time token generation with `IAsyncEnumerable<string>`
- **Fluent API**: Chainable configuration methods
- **Error Handling**: Comprehensive exception handling and device fallbacks
- **Performance**: Optimized for both throughput and latency

## Installation

### Prerequisites

1. **Install .NET 9.0 SDK or later**
   - Download from: https://dotnet.microsoft.com/download

2. **Install OpenVINO GenAI Runtime 2025.2.0.0**
   - Download from: https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/
   - Extract to a directory in your PATH, or place DLLs in your application's output directory

### Build from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/OpenVINO-C-Sharp.git
cd OpenVINO-C-Sharp

# Build the solution
dotnet build OpenVINO.NET.sln

# Run the quick demo
dotnet run --project samples/QuickDemo
```

## Performance Benchmarks

### Expected Performance (Qwen3-0.6B-fp16-ov)

| Device | Tokens/Second | First Token Latency | Notes |
|--------|---------------|-------------------|--------|
| CPU    | 12-15         | 400-600ms        | Always available |
| GPU    | 20-30         | 200-400ms        | Requires compatible GPU |
| NPU    | 15-25         | 300-500ms        | Intel NPU required |

### Benchmark Command

```bash
# Compare all available devices
dotnet run --project samples/QuickDemo -- --benchmark
```

## Troubleshooting

### Common Issues

#### 1. "OpenVINO runtime not found"
```
Error: The specified module could not be found. (Exception from HRESULT: 0x8007007E)
```
**Solution**: Ensure OpenVINO GenAI runtime DLLs are in your PATH or application directory.

#### 2. "Device not supported"
```
Error: Failed to create LLM pipeline on GPU: Device GPU is not supported
```
**Solutions**:
- Check device availability: `dotnet run --project samples/QuickDemo -- --benchmark`
- Use CPU fallback: `dotnet run --project samples/QuickDemo -- --device CPU`
- Install appropriate drivers (Intel GPU driver for GPU support, Intel NPU driver for NPU)

#### 3. "Model download fails"
```
Error: Failed to download model files from HuggingFace
```
**Solutions**:
- Check internet connectivity
- Verify HuggingFace is accessible
- Manually download model files to `./Models/Qwen3-0.6B-fp16-ov/`

#### 4. "Out of memory during inference"
```
Error: Insufficient memory to load model
```
**Solutions**:
- Use a smaller model
- Reduce max_tokens parameter
- Close other memory-intensive applications
- Consider using INT4 quantized models

### Debug Mode

Enable detailed logging by setting environment variable:
```bash
# Windows
set OPENVINO_LOG_LEVEL=DEBUG

# Linux/macOS
export OPENVINO_LOG_LEVEL=DEBUG
```

## Contributing

### Development Setup

1. **Install Prerequisites**
   - Visual Studio 2022 or VS Code with C# extension
   - .NET 9.0 SDK
   - OpenVINO GenAI runtime

2. **Build and Test**
   ```bash
   dotnet build OpenVINO.NET.sln
   dotnet test tests/OpenVINO.NET.GenAI.Tests/
   ```

3. **Code Style**
   - Follow Microsoft C# coding conventions
   - Use async/await patterns
   - Implement proper resource disposal (using statements)
   - Add XML documentation for public APIs

### Adding New Features

1. **P/Invoke Layer**: Add native method declarations in `GenAINativeMethods.cs`
2. **SafeHandle**: Create appropriate handle classes for resource management
3. **High-level API**: Implement user-friendly wrapper classes
4. **Tests**: Add comprehensive unit tests
5. **Documentation**: Update README and XML docs

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Resources

- [OpenVINO GenAI Documentation](https://docs.openvino.ai/2024/learn-openvino/llm_inference_guide.html)
- [OpenVINO GenAI C API Reference](https://docs.openvino.ai/2024/api/c_cpp_api/genai_group.html)
- [.NET P/Invoke Documentation](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [HuggingFace Model Hub](https://huggingface.co/OpenVINO)

## Building

```bash
dotnet build OpenVINO.NET.sln
```

