# Quick Start Guide

This guide will help you get up and running with OpenVINO.GenAI.NET quickly.

## Option 1: Quick Demo (Recommended)

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

## Option 2: Code Integration

For integrating into your own applications:

```csharp
using OpenVINO.NET.GenAI;

using var pipeline = new LLMPipeline("path/to/model", "CPU");
var config = GenerationConfig.Default.WithMaxTokens(100).WithTemperature(0.7f);

string result = await pipeline.GenerateAsync("Hello, world!", config);
Console.WriteLine(result);
```

## Streaming Generation

```csharp
using OpenVINO.NET.GenAI;

using var pipeline = new LLMPipeline("path/to/model", "CPU");
var config = GenerationConfig.Default.WithMaxTokens(100);

await foreach (var token in pipeline.GenerateStreamAsync("Tell me a story", config))
{
    Console.Write(token);
}
```

## Projects Overview

- `OpenVINO.NET.Core` - Core OpenVINO wrapper
- `OpenVINO.NET.GenAI` - GenAI functionality
- `OpenVINO.NET.Native` - Native library management
- `QuickDemo` - **Quick start demo with automatic model download**
- `TextGeneration.Sample` - Basic text generation example
- `StreamingChat.Sample` - Streaming chat application

## Next Steps

- Check out the [Architecture](architecture.md) to understand the design
- Review [Installation](installation.md) for detailed setup instructions
- See [Performance](performance.md) for benchmarking information
- Visit [Troubleshooting](troubleshooting.md) if you encounter issues
