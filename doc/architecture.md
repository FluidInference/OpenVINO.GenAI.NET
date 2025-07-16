# Architecture Overview

## C API Interoperability Layer

This is how it works, peeling the onion layers:

```
┌─────────────────────────────────────────────────────────────────┐
│                      .NET Application                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Your C# Code                               │    │
│  │  using var pipeline = new LLMPipeline("model", "CPU");  │    │
│  │  string result = await pipeline.GenerateAsync(...);     │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ C# Method Calls
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                OpenVINO.NET.GenAI Library                       │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │         Managed C# Wrapper Classes                      │    │
│  │  - LLMPipeline                                          │    │
│  │  - GenerationConfig                                     │    │
│  │  - WhisperPipeline (coming soon)                        │    │
│  │  - SafeHandles for memory management                    │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ P/Invoke Calls
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                   P/Invoke Layer                                │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              NativeMethods                              │    │
│  │  [DllImport("openvino_genai_c.dll")]                    │    │
│  │  public static extern ov_status_e                       │    │
│  │  ov_genai_llm_pipeline_create(...)                      │    │
│  │  ov_genai_llm_pipeline_generate(...)                    │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Native Function Calls
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    C API Layer                                  │
│                 (openvino_genai_c.dll)                          │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │         OpenVINO GenAI C API                            │    │
│  │  - ov_genai_llm_pipeline_create()                       │    │
│  │  - ov_genai_llm_pipeline_generate()                     │    │
│  │  - ov_genai_llm_pipeline_free()                         │    │
│  │  - Streaming callbacks & marshaling                     │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ C++ Function Calls
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                 OpenVINO GenAI Core                             │
│                 (openvino_genai.dll)                            │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │            C++ Implementation                           │    │
│  │  - LLMPipeline class                                    │    │
│  │  - Model loading & inference                            │    │
│  │  - Memory management                                    │    │
│  │  - Device optimization (CPU/GPU/NPU)                    │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Inference Calls
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                   OpenVINO Runtime                              │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Model Execution                            │    │
│  │  - CPU/GPU/NPU inference                                │    │
│  │  - Model optimization                                   │    │
│  │  - Hardware acceleration                                │    │
│  │  - Intel AIPC optimizations                             │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

**Key Architecture Points:**

1. **C API Bridge**: The C API (`openvino_genai_c.dll`) provides a stable ABI between managed .NET and unmanaged C++
2. **P/Invoke Layer**: Uses `DllImport` attributes to call native functions with proper marshaling
3. **Memory Safety**: SafeHandles and IDisposable patterns ensure proper cleanup of unmanaged resources
4. **Streaming Support**: Callback mechanisms for streaming inference results back to .NET
5. **Device Flexibility**: Supports CPU, GPU, and NPU devices through the same interface

This architecture allows your .NET applications to leverage OpenVINO's high-performance inference capabilities while maintaining the safety and convenience of managed code.
