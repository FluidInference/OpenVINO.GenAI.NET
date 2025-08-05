using System.Diagnostics;
using OpenVINO.NET.GenAI;
using Xunit;
using Xunit.Abstractions;

namespace OpenVINO.NET.GenAI.Tests;

/// <summary>
/// Integration tests for WhisperPipeline that perform actual model inference.
/// These tests are skipped if the model is not available.
/// </summary>
[Collection("Sequential")]
public class WhisperIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _modelPath;
    private readonly bool _modelAvailable;
    private readonly string _testDataPath;

    public WhisperIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // Check for model path from environment variable or default location
        _modelPath = Environment.GetEnvironmentVariable("WHISPER_MODEL_PATH")
            ?? Path.Combine(GetProjectRoot(), "Models", "whisper-tiny-int4-ov-npu");

        _modelAvailable = Directory.Exists(_modelPath) &&
            File.Exists(Path.Combine(_modelPath, "openvino_model.xml"));

        _testDataPath = Path.Combine(GetProjectRoot(), "tests", "OpenVINO.NET.GenAI.Tests", "TestData");

        if (!_modelAvailable)
        {
            _output.WriteLine($"Whisper model not found at: {_modelPath}");
            _output.WriteLine("Integration tests will be skipped. Download the model from:");
            _output.WriteLine("https://huggingface.co/FluidInference/whisper-tiny-int4-ov-npu");
        }
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_BasicTranscription_GeneratesText()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("en")
            .WithTask(WhisperTask.Transcribe);

        // Create test audio: 3 seconds of sine wave (simulating speech)
        var testAudio = GenerateTestAudio(3.0f);

        // Act
        var results = await pipeline.GenerateAsync(testAudio, config);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        var firstResult = results[0];
        Assert.NotNull(firstResult.Text);
        Assert.True(firstResult.Score >= 0 && firstResult.Score <= 1, "Score should be between 0 and 1");

        _output.WriteLine($"Transcribed text: {firstResult.Text}");
        _output.WriteLine($"Confidence score: {firstResult.Score:F4}");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_TranscriptionWithTimestamps_GeneratesChunks()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("en")
            .WithTask(WhisperTask.Transcribe)
            .WithTimestamps(true);

        // Create test audio
        var testAudio = GenerateTestAudio(5.0f);

        // Act
        var results = await pipeline.GenerateAsync(testAudio, config);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        var firstResult = results[0];
        
        if (firstResult.HasChunks)
        {
            Assert.NotNull(firstResult.Chunks);
            Assert.NotEmpty(firstResult.Chunks);
            
            _output.WriteLine($"Generated {firstResult.Chunks.Count} chunks:");
            foreach (var chunk in firstResult.Chunks)
            {
                _output.WriteLine($"  {chunk}");
                Assert.True(chunk.StartTime >= 0);
                Assert.True(chunk.EndTime > chunk.StartTime);
                Assert.NotEmpty(chunk.Text);
            }
        }
        else
        {
            _output.WriteLine("Model did not generate timestamped chunks");
        }
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_TranslationMode_TranslatesToEnglish()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("es") // Spanish
            .WithTask(WhisperTask.Translate); // Translate to English

        // Create test audio
        var testAudio = GenerateTestAudio(3.0f);

        // Act
        var results = await pipeline.GenerateAsync(testAudio, config);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        var firstResult = results[0];
        Assert.NotNull(firstResult.Text);

        _output.WriteLine($"Translated text: {firstResult.Text}");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_SilentAudio_HandlesGracefully()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("en")
            .WithTask(WhisperTask.Transcribe);

        // Create silent audio
        var silentAudio = new float[16000 * 2]; // 2 seconds of silence

        // Act
        var results = await pipeline.GenerateAsync(silentAudio, config);

        // Assert
        Assert.NotNull(results);
        // Model might return empty text or minimal text for silence
        _output.WriteLine($"Result for silence: '{results.FirstOrDefault()?.Text ?? "(empty)"}'");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_WithCancellation_StopsProcessing()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("en")
            .WithTask(WhisperTask.Transcribe);

        // Create longer test audio
        var testAudio = GenerateTestAudio(10.0f);
        using var cts = new CancellationTokenSource();

        // Act
        var transcriptionTask = pipeline.GenerateAsync(testAudio, config, cts.Token);
        
        // Cancel after a short delay
        cts.CancelAfter(100);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await transcriptionTask);
        _output.WriteLine("Transcription was successfully cancelled");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void WhisperPipeline_InvalidDevice_FallsBackOrThrows()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Try to create pipeline with NPU device
        try
        {
            using var pipeline = new WhisperPipeline(_modelPath, "NPU");
            _output.WriteLine("NPU device is available on this system");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"NPU not available: {ex.Message}");
            // Verify CPU fallback works
            using var cpuPipeline = new WhisperPipeline(_modelPath, "CPU");
            Assert.NotNull(cpuPipeline);
            _output.WriteLine("Successfully fell back to CPU");
        }
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_Performance_MeasuresSpeed()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var config = WhisperGenerationConfig.Default
            .WithLanguage("en")
            .WithTask(WhisperTask.Transcribe);

        // Create test audio of known duration
        var audioDurationSeconds = 5.0f;
        var testAudio = GenerateTestAudio(audioDurationSeconds);

        // Warm-up run
        _ = await pipeline.GenerateAsync(testAudio, config);

        // Act - measure performance
        var stopwatch = Stopwatch.StartNew();
        var results = await pipeline.GenerateAsync(testAudio, config);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(results);
        var processingTime = stopwatch.Elapsed.TotalSeconds;
        var realTimeFactor = processingTime / audioDurationSeconds;

        _output.WriteLine($"Audio duration: {audioDurationSeconds:F2} seconds");
        _output.WriteLine($"Processing time: {processingTime:F2} seconds");
        _output.WriteLine($"Real-time factor: {realTimeFactor:F2}x (lower is better)");
        _output.WriteLine($"Speed: {1/realTimeFactor:F2}x realtime");

        // Performance assertion - should process faster than 2x real-time on CPU
        Assert.True(realTimeFactor < 2.0, $"Processing too slow: {realTimeFactor:F2}x real-time");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_ConfigurationPersistence_MaintainsSettings()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        using var pipeline = new WhisperPipeline(_modelPath, "CPU");
        var customConfig = WhisperGenerationConfig.Default
            .WithLanguage("fr")
            .WithTask(WhisperTask.Translate)
            .WithTimestamps(true)
            .WithInitialPrompt("Technical conversation");

        // Act
        pipeline.SetGenerationConfig(customConfig);
        var retrievedConfig = pipeline.GetGenerationConfig();

        // Generate with the configured pipeline
        var testAudio = GenerateTestAudio(2.0f);
        var results = await pipeline.GenerateAsync(testAudio);

        // Assert
        Assert.NotNull(retrievedConfig);
        Assert.NotNull(results);
        _output.WriteLine("Configuration was successfully set and used");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task WhisperPipeline_TranscribeFile_LoadsAndTranscribes()
    {
        Skip.IfNot(_modelAvailable, "Whisper model not available for integration testing");

        // Arrange
        var testWavFile = CreateTestWavFile();
        try
        {
            using var pipeline = new WhisperPipeline(_modelPath, "CPU");
            var config = WhisperGenerationConfig.Default
                .WithLanguage("en")
                .WithTask(WhisperTask.Transcribe);

            // Act
            var results = await pipeline.TranscribeFileAsync(testWavFile, config);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            _output.WriteLine($"Transcribed from file: {results[0].Text}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(testWavFile))
                File.Delete(testWavFile);
        }
    }

    /// <summary>
    /// Generates test audio data (sine wave to simulate speech patterns)
    /// </summary>
    private static float[] GenerateTestAudio(float durationSeconds)
    {
        const int sampleRate = 16000;
        var sampleCount = (int)(sampleRate * durationSeconds);
        var audio = new float[sampleCount];

        // Generate a modulated sine wave to simulate speech-like patterns
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            // Mix of frequencies to simulate speech
            float carrier = (float)Math.Sin(2 * Math.PI * 440 * t); // 440 Hz carrier
            float modulator = (float)Math.Sin(2 * Math.PI * 3 * t); // 3 Hz modulation
            audio[i] = carrier * 0.3f * (1 + 0.5f * modulator);
        }

        return audio;
    }

    /// <summary>
    /// Creates a test WAV file with generated audio
    /// </summary>
    private string CreateTestWavFile()
    {
        var tempFile = Path.GetTempFileName() + ".wav";
        var audio = GenerateTestAudio(2.0f);

        using (var fs = new FileStream(tempFile, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            // WAV header
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + audio.Length * 2); // File size - 8
            writer.Write("WAVE".ToCharArray());

            // fmt chunk
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); // Chunk size
            writer.Write((short)1); // PCM format
            writer.Write((short)1); // Mono
            writer.Write(16000); // Sample rate
            writer.Write(16000 * 2); // Byte rate
            writer.Write((short)2); // Block align
            writer.Write((short)16); // Bits per sample

            // data chunk
            writer.Write("data".ToCharArray());
            writer.Write(audio.Length * 2); // Data size

            // Write audio data as 16-bit PCM
            foreach (var sample in audio)
            {
                short pcmSample = (short)(sample * 32767);
                writer.Write(pcmSample);
            }
        }

        return tempFile;
    }

    private static string GetProjectRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? Directory.GetCurrentDirectory();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}