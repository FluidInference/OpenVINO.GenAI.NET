# OpenVINO.NET Project Memory

## Project Goals and Requirements

### Primary Goal
Create a comprehensive C# wrapper for OpenVINO and OpenVINO GenAI C code to make it more usable directly in C# applications, compared to basic P/Invoke examples.

### Core Requirements
- **Target Platform**: Windows x64, .NET 6.0+
- **OpenVINO Version**: OpenVINO GenAI 2025.2.0.0-rc4 (required for needed C API changes)
- **Modern C# Patterns**: Async/await, IAsyncEnumerable, SafeHandle resource management
- **Simple Demo**: CLI with hardcoded values, device selection, and benchmark mode
- **Automatic Setup**: Model download from HuggingFace, native DLL deployment

## Architecture Overview

### Three-Layer Design
1. **OpenVINO.NET.Core**: Core OpenVINO functionality
2. **OpenVINO.NET.GenAI**: GenAI-specific features (LLM pipelines, streaming)
3. **OpenVINO.NET.Native**: Native library management and MSBuild targets

### Key Technical Decisions
- **P/Invoke Layer**: `GenAINativeMethods.cs` with proper marshalling
- **SafeHandle Pattern**: `LLMPipelineHandle`, `GenerationConfigHandle` for resource management
- **Streaming Support**: `IAsyncEnumerable<string>` for token-by-token generation
- **Fluent API**: `GenerationConfig.Default.WithMaxTokens(100).WithTemperature(0.7f)`
- **MSBuild Integration**: Automatic native DLL deployment via `.targets` files

## Important File Locations

### Core Implementation
- `src/OpenVINO.NET.GenAI/Native/GenAINativeMethods.cs` - P/Invoke declarations
- `src/OpenVINO.NET.GenAI/LLMPipeline.cs` - Main high-level API
- `src/OpenVINO.NET.GenAI/GenerationConfig.cs` - Configuration with fluent API
- `src/OpenVINO.NET.GenAI/OpenVINO.NET.GenAI.targets` - MSBuild targets

### Demo Application
- `samples/QuickDemo/Program.cs` - Simple CLI demo with hardcoded values
- Model: `OpenVINO/Qwen3-0.6B-fp16-ov` (1.2GB download from HuggingFace)

## Build and Test Commands

```bash
# Build entire solution
dotnet build OpenVINO.NET.sln

# Run QuickDemo (default CPU)
dotnet run --project samples/QuickDemo

# Run on specific device
dotnet run --project samples/QuickDemo -- --device GPU

# Benchmark all devices
dotnet run --project samples/QuickDemo -- --benchmark
```

## Key Configuration Values

### QuickDemo Settings (Hardcoded)
- **Model**: `OpenVINO/Qwen3-0.6B-fp16-ov`
- **Temperature**: 0.7f
- **Max Tokens**: 100
- **Top-P**: 0.9f
- **Devices**: CPU, GPU, NPU

### Test Prompts
1. "Explain quantum computing in simple terms:"
2. "Write a short poem about artificial intelligence:"
3. "What are the benefits of renewable energy?"
4. "Describe the process of making coffee:"

## Common Issues and Solutions

### .NET 6 Compatibility
- **Issue**: `ArgumentException.ThrowIfNullOrEmpty` not available
- **Solution**: Use traditional null checks:
```csharp
if (string.IsNullOrEmpty(value))
    throw new ArgumentException("Value cannot be null or empty", nameof(value));
```

### Native Library Path
- **Issue**: MSBuild targets path warnings on non-Windows
- **Solution**: Conditional inclusion in `.targets` file

## Performance Expectations
- **CPU**: ~12-15 tokens/sec
- **GPU**: ~20-30 tokens/sec (if available)
- **NPU**: ~15-25 tokens/sec (if available)
- **First Token Latency**: 400-800ms

## User Feedback Patterns
- **Preference**: Simple, hardcoded demos over complex configuration
- **Focus**: Device selection and benchmark comparisons
- **Avoid**: Overly complex CLI interfaces
- **Emphasize**: "Just works" experience with automatic model download