# OpenVINO.NET

A comprehensive C# wrapper for OpenVINO and OpenVINO GenAI, providing idiomatic .NET APIs for AI inference and generative AI tasks.

## Features

- **OpenVINO.NET.Core**: Core OpenVINO functionality for model inference
- **OpenVINO.NET.GenAI**: Generative AI capabilities including LLM pipelines
- **OpenVINO.NET.Native**: Native library management and deployment
- **Modern C# API**: Async/await, IAsyncEnumerable, SafeHandle resource management
- **Windows x64 Support**: Optimized for Windows deployment scenarios

## Requirements

- .NET 6.0 or later
- Windows x64
- OpenVINO GenAI 2025.2.0.0-rc4 runtime

## Quick Start

### Text Generation

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
- `TextGeneration.Sample` - Basic text generation example
- `StreamingChat.Sample` - Streaming chat application

## Building

```bash
dotnet build OpenVINO.NET.sln
```

