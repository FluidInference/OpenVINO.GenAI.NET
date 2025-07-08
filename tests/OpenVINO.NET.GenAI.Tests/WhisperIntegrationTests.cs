using Xunit;
using OpenVINO.NET.GenAI;
using System.Diagnostics;

namespace OpenVINO.NET.GenAI.Tests;

/// <summary>
/// Integration tests for Whisper speech transcription functionality
/// </summary>
public class WhisperIntegrationTests : IDisposable
{
    private readonly string? _modelPath;
    private readonly bool _skipTests;

    public WhisperIntegrationTests()
    {
        // Check for environment variable (used in CI) or default location
        _modelPath = Environment.GetEnvironmentVariable("WHISPERDEMO_MODEL_PATH")
                     ?? Path.Combine("Models", "whisper-tiny.en");

        _skipTests = !Directory.Exists(_modelPath) || !Directory.GetFiles(_modelPath, "*.xml").Any();
    }

    /// <summary>
    /// Generates test audio data (sine wave at 440Hz for 2 seconds)
    /// </summary>
    private static float[] GenerateTestAudio(int durationSeconds = 2, int sampleRate = 16000, double frequency = 440.0)
    {
        var samples = new float[sampleRate * durationSeconds];
        for (int i = 0; i < samples.Length; i++)
        {
            var time = (double)i / sampleRate;
            samples[i] = (float)(0.3 * Math.Sin(2 * Math.PI * frequency * time));
        }
        return samples;
    }

    [SkippableFact]
    public void WhisperPipeline_Constructor_WithValidParameters_ShouldSucceed()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        Assert.NotNull(pipeline);
    }

    [SkippableFact]
    public void WhisperPipeline_Constructor_WithInvalidModelPath_ShouldThrow()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        Assert.Throws<ArgumentException>(() => new WhisperPipeline("", "CPU"));
        Assert.Throws<ArgumentException>(() => new WhisperPipeline(_modelPath!, ""));
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithBasicConfig_ShouldReturnResult()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        var audioData = GenerateTestAudio();

        var result = pipeline.Generate(audioData, config);

        Assert.NotNull(result);
        Assert.NotNull(result.Text);
        Assert.True(result.Texts.Length > 0);
        Assert.NotNull(result.PerformanceMetrics);

        result.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public async Task WhisperPipeline_GenerateAsync_WithBasicConfig_ShouldReturnResult()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        var audioData = GenerateTestAudio();

        var result = await pipeline.GenerateAsync(audioData, config);

        Assert.NotNull(result);
        Assert.NotNull(result.Text);
        Assert.True(result.Texts.Length > 0);

        result.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithTimestamps_ShouldReturnChunks()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish().WithTimestamps(true);
        var audioData = GenerateTestAudio();

        var result = pipeline.Generate(audioData, config);

        Assert.NotNull(result);
        Assert.NotNull(result.Text);

        result.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithInitialPrompt_ShouldReturnResult()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default
            .ForTranscriptionEnglish()
            .WithInitialPrompt("This is a test audio signal.");
        var audioData = GenerateTestAudio();

        var result = pipeline.Generate(audioData, config);

        Assert.NotNull(result);
        Assert.NotNull(result.Text);

        result.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public async Task WhisperPipeline_GenerateStreamAsync_ShouldReturnChunks()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish().WithTimestamps(true);
        var audioData = GenerateTestAudio();

        var chunks = new List<WhisperChunk>();
        await foreach (var chunk in pipeline.GenerateStreamAsync(audioData, config))
        {
            chunks.Add(chunk);
            Assert.NotNull(chunk.Text);
            Assert.True(chunk.StartTime >= 0);
            Assert.True(chunk.EndTime > chunk.StartTime);
            Assert.True(chunk.Duration > 0);
        }

        config.Dispose();
    }

    [SkippableFact]
    public async Task WhisperPipeline_GenerateStreamAsync_WithCancellation_ShouldCancel()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        var audioData = GenerateTestAudio(5); // Longer audio

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        var chunks = new List<WhisperChunk>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var chunk in pipeline.GenerateStreamAsync(audioData, config, cts.Token))
            {
                chunks.Add(chunk);
            }
        });

        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_SetAndGetGenerationConfig_ShouldWork()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();

        pipeline.SetGenerationConfig(config);
        var retrievedConfig = pipeline.GetGenerationConfig();

        Assert.NotNull(retrievedConfig);

        retrievedConfig.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithEmptyAudio_ShouldThrow()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        var emptyAudio = Array.Empty<float>();

        Assert.Throws<ArgumentException>(() => pipeline.Generate(emptyAudio, config));

        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithInvalidAudio_ShouldThrow()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        var invalidAudio = new float[] { float.NaN, float.PositiveInfinity, 0.5f };

        Assert.Throws<ArgumentException>(() => pipeline.Generate(invalidAudio, config));

        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_Generate_WithTooLongAudio_ShouldThrow()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish();
        // Generate 31 seconds of audio (exceeds 30-second limit)
        var tooLongAudio = GenerateTestAudio(31);

        Assert.Throws<ArgumentException>(() => pipeline.Generate(tooLongAudio, config));

        config.Dispose();
    }

    [SkippableTheory]
    [InlineData("CPU")]
    [InlineData("GPU")]
    [InlineData("NPU")]
    public void WhisperPipeline_BenchmarkDevices_ShouldMeasurePerformance(string device)
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        try
        {
            using var pipeline = new WhisperPipeline(_modelPath!, device);
            var config = WhisperConfig.Default.ForTranscriptionEnglish();
            var audioData = GenerateTestAudio();

            var stopwatch = Stopwatch.StartNew();
            var result = pipeline.Generate(audioData, config);
            stopwatch.Stop();

            Assert.NotNull(result);
            Assert.True(stopwatch.ElapsedMilliseconds > 0);

            var audioSeconds = audioData.Length / 16000.0;
            var realTimeFactor = stopwatch.Elapsed.TotalSeconds / audioSeconds;

            // Log performance for debugging
            var output = $"Device: {device}, RTF: {realTimeFactor:F2}x, Duration: {stopwatch.ElapsedMilliseconds}ms";
            Console.WriteLine(output);

            result.Dispose();
            config.Dispose();
        }
        catch (Exception ex) when (device != "CPU")
        {
            // Skip test for non-CPU devices if they're not available
            Skip.IfNot(false, $"{device} device not available: {ex.Message}");
        }
    }

    [SkippableFact]
    public void WhisperResult_Properties_ShouldBeAccessible()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        using var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        var config = WhisperConfig.Default.ForTranscriptionEnglish().WithTimestamps(true);
        var audioData = GenerateTestAudio();

        var result = pipeline.Generate(audioData, config);

        Assert.NotNull(result.Text);
        Assert.NotNull(result.Texts);
        Assert.NotNull(result.Scores);
        Assert.NotNull(result.PerformanceMetrics);

        // Test performance metrics
        var metrics = result.PerformanceMetrics;
        var (mean, stdDev) = metrics.FeaturesExtractionDuration;
        Assert.True(mean >= 0);
        Assert.True(stdDev >= 0);
        Assert.NotNull(metrics.ToString());

        result.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public void WhisperPipeline_DisposedAccess_ShouldThrow()
    {
        Skip.IfNot(!_skipTests, "Whisper model not available");

        var pipeline = new WhisperPipeline(_modelPath!, "CPU");
        pipeline.Dispose();

        var audioData = GenerateTestAudio();
        Assert.Throws<ObjectDisposedException>(() => pipeline.Generate(audioData));
    }

    public void Dispose()
    {
        // Cleanup any resources if needed
    }
}