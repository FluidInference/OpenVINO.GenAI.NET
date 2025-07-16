# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

OpenVINO.NET is a comprehensive C# wrapper for OpenVINO and OpenVINO GenAI C APIs, providing idiomatic .NET APIs for AI inference and generative AI tasks. The project targets Windows x64 with .NET 9.0+ and emphasizes modern C# patterns including async/await, SafeHandle resource management, and streaming generation.

## Essential Development Commands

### Building and Testing
```bash
# Build entire solution
dotnet build OpenVINO.NET.sln

# Run tests
dotnet test tests/OpenVINO.NET.GenAI.Tests/

# Run specific test project
dotnet test tests/OpenVINO.NET.GenAI.Tests/OpenVINO.NET.GenAI.Tests.csproj

# Build in release mode
dotnet build OpenVINO.NET.sln -c Release
```

### Sample Applications
```bash
# QuickDemo - Main demo with automatic model download
dotnet run --project samples/QuickDemo                    # Default CPU
dotnet run --project samples/QuickDemo -- --device GPU    # Specific device
dotnet run --project samples/QuickDemo -- --benchmark     # Compare all devices

# Other samples
dotnet run --project samples/TextGeneration              # Basic text generation
dotnet run --project samples/StreamingChat               # Chat with streaming
dotnet run --project samples/WhisperDemo                 # Speech-to-text
```

### Package Management
```bash
# Create NuGet package
dotnet pack src/OpenVINO.NET.GenAI/ -c Release

# Install from local package
dotnet add package OpenVINO.NET.GenAI --source ./nuget/
```

## Architecture Overview

### Three-Layer Design

The project follows a clear separation of concerns across three main layers:

1. **OpenVINO.NET.Core** - Core OpenVINO functionality and base types
2. **OpenVINO.NET.GenAI** - High-level generative AI APIs (LLM, Whisper)
3. **OpenVINO.NET.Native** - Native library management and MSBuild integration

### Key Components

**P/Invoke Layer** (`src/OpenVINO.NET.GenAI/Native/`):
- `GenAINativeMethods.cs` - P/Invoke declarations for OpenVINO GenAI C API
- `NativeStructures.cs` - C struct definitions and marshalling
- Uses `openvino_genai_c` as the native library name

**SafeHandle Resource Management** (`src/OpenVINO.NET.GenAI/SafeHandles/`):
- `LLMPipelineSafeHandle.cs` - LLM pipeline resource management
- `GenerationConfigSafeHandle.cs` - Configuration object management
- `WhisperPipelineSafeHandle.cs` - Whisper pipeline management
- All handles follow proper disposal patterns

**High-Level APIs**:
- `LLMPipeline.cs` - Main text generation API with sync/async methods
- `GenerationConfig.cs` - Fluent configuration API
- `ChatSession.cs` - Conversation management
- `WhisperPipeline.cs` - Speech-to-text processing

**MSBuild Integration**:
- `OpenVINO.NET.GenAI.targets` - Automatic native DLL deployment
- Copies DLLs from `build/native/runtimes/win-x64/native/` to output directory
- Handles both build and publish scenarios

### Modern C# Patterns

**Async/Await Support**:
- `GenerateAsync()` for async text generation
- `GenerateStreamAsync()` returning `IAsyncEnumerable<string>`
- Proper cancellation token support throughout

**Resource Management**:
- SafeHandle pattern for all native resources
- Automatic cleanup on disposal
- Exception-safe resource handling

**Fluent API Design**:
```csharp
var config = GenerationConfig.Default
    .WithMaxTokens(100)
    .WithTemperature(0.7f)
    .WithTopP(0.9f);
```

## Key Configuration and Constants

### QuickDemo Application Settings
- **Model**: `OpenVINO/Qwen3-0.6B-fp16-ov` (1.2GB download from HuggingFace)
- **Temperature**: 0.7f
- **Max Tokens**: 100
- **Top-P**: 0.9f
- **Supported Devices**: CPU, GPU, NPU

### Test Environment
- **Framework**: xUnit with `Xunit.SkippableFact` for conditional tests
- **Models Directory**: `./Models/` (relative to sample output)
- **Native Runtime**: OpenVINO GenAI 2025.2.0.0-rc4

## Performance Expectations

### LLM Generation (Qwen3-0.6B-fp16-ov)
- **CPU**: ~12-15 tokens/sec
- **GPU**: ~20-30 tokens/sec (Intel GPU required)
- **NPU**: ~15-25 tokens/sec (Intel NPU required)
- **First Token Latency**: 400-800ms

### Whisper Speech Recognition
- Real-time processing capabilities
- Supports multiple audio formats
- Configurable language detection

## Common Development Patterns

### Error Handling
- `OpenVINOGenAIException` for native API errors
- `OpenVINOGenAIException.ThrowIfError()` for status checking
- Device fallback patterns in sample applications

### Native Library Management
- MSBuild targets handle DLL deployment automatically
- Runtime path: `build/native/runtimes/win-x64/native/`
- CI environment support with `$(CI)` variable

### Model Management
- Automatic download from HuggingFace Hub
- Local caching in `./Models/` directory
- Model path validation and error handling

## Testing Strategy

### Unit Tests
- Configuration object validation
- Exception handling scenarios
- SafeHandle lifecycle management

### Integration Tests
- End-to-end pipeline testing
- Device-specific functionality
- Model loading and inference

### Conditional Testing
- Use `SkippableFact` for hardware-dependent tests
- Environment variable checks for CI/CD
- Model availability validation

## Dependencies and Requirements

### Runtime Dependencies
- OpenVINO GenAI 2025.2.0.0-rc4 C API
- System.Memory (4.5.5)
- System.Runtime.CompilerServices.Unsafe (6.0.0)
- System.Threading.Channels (7.0.0)

### Development Dependencies
- .NET 9.0 SDK
- xUnit testing framework
- Visual Studio 2022 or VS Code with C# extension

### Platform Requirements
- Windows x64 (primary target)
- Intel hardware with OpenVINO support
- Optional: Intel GPU drivers for GPU inference
- Optional: Intel NPU drivers for NPU inference