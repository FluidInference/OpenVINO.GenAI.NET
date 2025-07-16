# OpenVINO.GenAI.NET | OpenVINO and OpenVINO GenAI interop in .NET for GenAI workloads

**This is very much a work in progress. Please do not use yet :). If this is something that might be helpful, leave an issue and let us know your use case and we can try to incorporate it as needed, otherwise the main focus will be on Windows .NET apps**

Inspired by this [blog](https://blog.openvino.ai/blog-posts/openvino-genai-delivers-c-api-for-seamless-language-interop-with-practical-examples-in-net) for interoperability with .NET applications using OpenVINO. Our team was trying to develop a Windows application that needed to use OpenVINO, but we found that support for C# was lacking and bundling a whole Python runtime with the app was just not feasible. With the C API, it would be simpler to support OpenVINO GenAI in C#.

Currently we have only implemented support for LLMPipeline; WhisperPipeline is coming soon [pr](https://github.com/openvinotoolkit/openvino.genai/pull/2414).

Our goal is to target Intel AIPCs, so we will only be officially supporting .NET 9.0 on Windows 11 for LLM and Whisper workloads.

If you need additional support, please join our Discord or reach out directly to discuss further.

## Requirements

- .NET 9.0 or later
- Windows 11
- Windows x64
- OpenVINO GenAI 2025.2.0.0-rc4 +

## Quick Start

The easiest way to get started is with the QuickDemo application that automatically downloads a model:

```bash
# Run with default CPU device
dotnet run --project samples/QuickDemo

# Run on specific device
dotnet run --project samples/QuickDemo -- --device GPU

# Compare performance across all devices
dotnet run --project samples/QuickDemo -- --benchmark
```

For integrating into your own applications:

```csharp
using OpenVINO.NET.GenAI;

using var pipeline = new LLMPipeline("path/to/model", "CPU");
var config = GenerationConfig.Default.WithMaxTokens(100).WithTemperature(0.7f);

string result = await pipeline.GenerateAsync("Hello, world!", config);
Console.WriteLine(result);
```

## Resources

- [OpenVINO GenAI Documentation](https://docs.openvino.ai/2024/learn-openvino/llm_inference_guide.html)
- [OpenVINO GenAI C API Reference](https://docs.openvino.ai/2024/api/c_cpp_api/genai_group.html)
- [.NET P/Invoke Documentation](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [HuggingFace Model Hub](https://huggingface.co/OpenVINO)

## Coming

Nuget package, Whisper pipeline, pyannote models for NPU"

## Alternatives

If you're looking to run YOLO or other traditional computer vision models, consider these alternative OpenVINO .NET bindings:

- **[OpenVINO.NET](https://github.com/sdcb/OpenVINO.NET)** - Comprehensive OpenVINO bindings for .NET
- **[OpenVINO-CSharp-API](https://github.com/guojin-yan/OpenVINO-CSharp-API)** - C# API for OpenVINO inference

## License

This project is licensed under the Apache 2.0 License - see the [LICENSE](LICENSE) file for details. It follows the original openvino.genai repo.
