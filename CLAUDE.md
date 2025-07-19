# OpenVINO.NET Project Memory

## Project Goals and Requirements

### Primary Goal
Create a comprehensive C# wrapper for OpenVINO and OpenVINO GenAI C code to make it more usable directly in C# applications, compared to basic P/Invoke examples.

### Core Requirements
- **Target Platform**: Windows x64, .NET 8.0+
- **OpenVINO Version**: OpenVINO GenAI 2025.2.0.0 (stable release with all needed C API changes)
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

# Run QuickDemo (default CPU) - Linux requires LD_LIBRARY_PATH
cd samples/QuickDemo/bin/Debug/net8.0 && LD_LIBRARY_PATH=. dotnet QuickDemo.dll

# Alternative: Build and run from project directory
dotnet build samples/QuickDemo && cd samples/QuickDemo/bin/Debug/net8.0 && LD_LIBRARY_PATH=. dotnet QuickDemo.dll

# Run on specific device (Linux)
cd samples/QuickDemo/bin/Debug/net8.0 && LD_LIBRARY_PATH=. dotnet QuickDemo.dll --device GPU

# Benchmark all devices (Linux)
cd samples/QuickDemo/bin/Debug/net8.0 && LD_LIBRARY_PATH=. dotnet QuickDemo.dll --benchmark

# Windows (no special environment needed)
dotnet run --project samples/QuickDemo
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

### Native Library Loading (Updated 2025.2)
- **Approach**: Simplified to use standard .NET library resolution with recursive search
- **Windows**: Automatic loading via SetDllDirectory() API
- **Linux**: Requires LD_LIBRARY_PATH=. when running from output directory
- **Libraries**: Uses official OpenVINO GenAI C API (`openvino_genai_c.dll/.so`)
- **Dependencies**: All dependencies deployed via MSBuild targets
- **Custom Installations**: Set `OPENVINO_RUNTIME_PATH` environment variable to point to your OpenVINO runtime directory

### Environment Variables
- **OPENVINO_RUNTIME_PATH**: Optional environment variable to specify custom OpenVINO runtime directory
  - Use this for non-standard OpenVINO installations or different architectures
  - Points to the directory containing `openvino_genai_c.dll` (Windows) or `libopenvino_genai_c.so` (Linux)
  - Example: `export OPENVINO_RUNTIME_PATH="/opt/intel/openvino/runtime/bin/intel64"`
  - If not set, the system will auto-discover libraries in the application directory and subdirectories

### .NET 8 Features
- **Modern C# Features**: Takes advantage of latest language features and performance improvements
- **Enhanced Performance**: Benefits from .NET 8 runtime optimizations for better inference speed
- **Native AOT Ready**: Can be compiled to native code for faster startup times

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

## Project Configuration and Environment Notes
- Remember the important configuration, environment and test changes

## Documentation and Publishing Steps
- Add documentation steps for creating and publishing documentation for the project
- Steps to follow for updating and maintaining project documentation