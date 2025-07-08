# OpenVINO.NET Whisper Support

This document describes the Whisper speech transcription functionality added to OpenVINO.NET.

## Overview

OpenVINO.NET now supports Whisper-based speech transcription through a comprehensive C# API that mirrors the existing LLM pipeline architecture. The implementation provides:

- **Synchronous and asynchronous transcription**
- **Real-time streaming support**  
- **Fluent configuration API**
- **Automatic audio preprocessing**
- **Performance metrics and benchmarking**
- **Multi-device support (CPU, GPU, NPU)**

## Quick Start

### Basic Transcription

```csharp
using OpenVINO.NET.GenAI;

// Initialize Whisper pipeline
using var pipeline = new WhisperPipeline("path/to/whisper/model", "CPU");

// Prepare audio data (16kHz, float32)
float[] audioData = AudioProcessor.PrepareForWhisper(rawAudio, sourceSampleRate);

// Configure transcription
var config = WhisperConfig.Default
    .ForTranscriptionEnglish()
    .WithTimestamps(true);

// Generate transcription
var result = await pipeline.GenerateAsync(audioData, config);

Console.WriteLine($"Transcription: {result.Text}");
foreach (var chunk in result.Chunks)
{
    Console.WriteLine($"[{chunk.StartTime:F1}s-{chunk.EndTime:F1}s]: {chunk.Text}");
}
```

### Streaming Transcription

```csharp
// Real-time streaming transcription
await foreach (var chunk in pipeline.GenerateStreamAsync(audioData, config))
{
    Console.WriteLine($"Live: [{chunk.StartTime:F1}s] {chunk.Text}");
}
```

## API Reference

### WhisperPipeline

The main entry point for speech transcription.

```csharp
public class WhisperPipeline : IDisposable
{
    // Constructor
    public WhisperPipeline(string modelPath, string device = "CPU")
    
    // Synchronous transcription
    public WhisperResult Generate(float[] audioData, WhisperConfig? config = null)
    
    // Asynchronous transcription
    public Task<WhisperResult> GenerateAsync(float[] audioData, WhisperConfig? config = null, CancellationToken cancellationToken = default)
    
    // Streaming transcription
    public IAsyncEnumerable<WhisperChunk> GenerateStreamAsync(float[] audioData, WhisperConfig? config = null, CancellationToken cancellationToken = default)
}
```

### WhisperConfig

Fluent configuration API for customizing transcription behavior.

```csharp
// Language-specific transcription
var config = WhisperConfig.Default
    .WithLanguage("en")           // Language code
    .WithTask("transcribe")       // "transcribe" or "translate"
    .WithTimestamps(true)         // Include timing information
    .WithInitialPrompt("Hello")   // Context prompt
    .WithHotwords("OpenVINO,AI"); // Emphasized words

// Convenience methods
WhisperConfig.Default.ForTranscriptionEnglish()
WhisperConfig.Default.ForTranslationToEnglish()
WhisperConfig.Default.ForTranscriptionLanguage("es")
```

### WhisperResult

Comprehensive transcription results with performance metrics.

```csharp
public class WhisperResult : IDisposable
{
    public string[] Texts { get; }                           // All transcriptions
    public float[] Scores { get; }                          // Confidence scores
    public WhisperChunk[] Chunks { get; }                   // Timestamped segments
    public WhisperPerformanceMetrics PerformanceMetrics { get; } // Performance data
    public string Text { get; }                             // Primary transcription
    public string FullText { get; }                         // Text with timestamps
}
```

### WhisperChunk

Individual transcription segment with timing.

```csharp
public class WhisperChunk
{
    public float StartTime { get; }     // Start time in seconds
    public float EndTime { get; }       // End time in seconds  
    public string Text { get; }         // Transcribed text
    public float Duration { get; }      // Segment duration
}
```

### AudioProcessor

Utility class for audio preprocessing and format conversion.

```csharp
public static class AudioProcessor
{
    // Convert formats
    public static float[] ConvertPcm16ToFloat32(short[] pcmData)
    public static float[] ConvertPcm16ToFloat32(byte[] pcmData)
    
    // Audio processing
    public static float[] Normalize(float[] audioData)
    public static float[] Resample(float[] audioData, int sourceSampleRate, int targetSampleRate = 16000)
    public static float[] StereoToMono(float[] stereoData)
    public static float[] TrimSilence(float[] audioData, float threshold = 0.01f)
    public static float[] PadOrTruncate(float[] audioData, int targetLength, float padValue = 0.0f)
    
    // Validation and preparation
    public static void ValidateAudioData(float[] audioData, int sampleRate)
    public static float[] PrepareForWhisper(float[] audioData, int sourceSampleRate, bool normalize = true)
}
```

## Audio Requirements

Whisper models expect specific audio format:

- **Sample Rate**: 16kHz
- **Format**: Float32 normalized to [-1.0, 1.0]
- **Channels**: Mono (single channel)
- **Maximum Duration**: 30 seconds (480,000 samples)

Use `AudioProcessor.PrepareForWhisper()` to automatically convert audio to the correct format.

## Model Support

The implementation supports OpenVINO-optimized Whisper models with the following files:

- `openvino_encoder_model.xml/bin` - Audio encoder
- `openvino_decoder_model.xml/bin` - Initial decoder  
- `openvino_decoder_with_past_model.xml/bin` - Decoder with attention cache
- `config.json` - Model configuration
- `preprocessor_config.json` - Audio preprocessing config

## Demo Application

The `WhisperDemo` sample application showcases all features:

```bash
# Basic transcription on CPU
dotnet run --project samples/WhisperDemo

# Run on specific device
dotnet run --project samples/WhisperDemo -- --device GPU

# Demonstrate streaming
dotnet run --project samples/WhisperDemo -- --streaming

# Benchmark all devices
dotnet run --project samples/WhisperDemo -- --benchmark
```

## Performance Considerations

- **CPU**: Good baseline performance for most applications
- **GPU**: Faster processing for longer audio files
- **NPU**: Optimized for edge deployment with low power consumption
- **Real-time Factor**: Values < 1.0 indicate faster-than-real-time processing

## Error Handling

The API provides comprehensive error handling:

- `OpenVINOGenAIException` for OpenVINO-specific errors
- `ArgumentException` for invalid audio data or configuration
- `ObjectDisposedException` for disposed objects

## Thread Safety

- `WhisperPipeline` instances are **not** thread-safe
- Create separate instances for concurrent operations
- `WhisperConfig` and result objects are immutable after creation

## Integration Examples

### From File

```csharp
// Load and process audio file
var audioBytes = File.ReadAllBytes("audio.wav");
var audioData = AudioProcessor.ConvertPcm16ToFloat32(audioBytes);
var processedAudio = AudioProcessor.PrepareForWhisper(audioData, 44100);

var result = await pipeline.GenerateAsync(processedAudio);
```

### From Microphone

```csharp
// Assuming you have microphone data as short[]
short[] microphoneData = GetMicrophoneData();
var audioData = AudioProcessor.ConvertPcm16ToFloat32(microphoneData);
var normalizedAudio = AudioProcessor.Normalize(audioData);

await foreach (var chunk in pipeline.GenerateStreamAsync(normalizedAudio))
{
    ProcessTranscriptionChunk(chunk);
}
```

### Multi-language Support

```csharp
var languages = new[] { "en", "es", "fr", "de" };

foreach (var lang in languages)
{
    var config = WhisperConfig.Default.ForTranscriptionLanguage(lang);
    var result = await pipeline.GenerateAsync(audioData, config);
    Console.WriteLine($"{lang}: {result.Text}");
}
```

## Advanced Configuration

### Custom Token Suppression

```csharp
var config = WhisperConfig.Default
    .WithSuppressTokens(new long[] { 50257, 50358 })           // Suppress specific tokens
    .WithBeginSuppressTokens(new long[] { 50257 })             // Suppress at beginning
    .WithDecoderStartTokenId(50258)                            // Custom start token
    .WithMaxInitialTimestampIndex(50);                         // Timestamp constraints
```

### Performance Monitoring

```csharp
var result = await pipeline.GenerateAsync(audioData);
var metrics = result.PerformanceMetrics;

Console.WriteLine($"Features Extraction: {metrics.FeaturesExtractionDuration.Mean:F2}ms");
```

This implementation provides a production-ready foundation for integrating Whisper speech transcription into .NET applications using OpenVINO acceleration.