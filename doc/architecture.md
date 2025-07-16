# Architecture Overview

## Three-Layer Design

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
│  • Version: 2025.2.0.0-rc4                                 │
└─────────────────────────────────────────────────────────────┘
```

## Key Features

- **Memory Safe**: SafeHandle pattern for automatic resource cleanup
- **Async/Await**: Full async support with cancellation tokens
- **Streaming**: Real-time token generation with `IAsyncEnumerable<string>`
- **Fluent API**: Chainable configuration methods
- **Error Handling**: Comprehensive exception handling and device fallbacks
- **Performance**: Optimized for both throughput and latency

## Layer Details

### Your Application Layer
The top layer where you build your applications using the high-level APIs provided by OpenVINO.NET.GenAI.

### OpenVINO.NET.GenAI
This is the main user-facing library that provides:
- **LLMPipeline**: High-level API for text generation
- **GenerationConfig**: Fluent configuration for controlling generation parameters
- **ChatSession**: Manages conversation state and history
- **Streaming Support**: Real-time token generation through `IAsyncEnumerable<string>`

### OpenVINO.NET.Core
The core library that handles:
- Model loading and management
- OpenVINO inference operations
- Device management and selection

### OpenVINO.NET.Native
The native interop layer that provides:
- P/Invoke declarations for calling native code
- SafeHandle implementations for automatic resource management
- MSBuild targets for deploying native DLLs

### OpenVINO GenAI C API
The underlying native OpenVINO GenAI runtime that performs the actual inference operations.

## Design Principles

1. **Safety First**: All native resources are managed through SafeHandle patterns
2. **Async by Design**: Full async/await support throughout the API
3. **Streaming Support**: Real-time token generation for responsive UIs
4. **Fluent Configuration**: Chainable methods for easy configuration
5. **Error Resilience**: Comprehensive error handling and device fallbacks
