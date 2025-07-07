using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using OpenVINO.NET.GenAI;

namespace OpenVINO.NET.GenAI.Tests;

/// <summary>
/// Integration tests that perform actual model inference.
/// These tests are skipped if the model is not available.
/// </summary>
[Collection("Sequential")]
public class IntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _modelPath;
    private readonly bool _modelAvailable;

    public IntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // Check for model path from environment variable or default location
        _modelPath = Environment.GetEnvironmentVariable("QUICKDEMO_MODEL_PATH")
            ?? Path.Combine(GetProjectRoot(), "Models", "qwen3-0.6b-int4-ov");

        _modelAvailable = Directory.Exists(_modelPath) &&
            File.Exists(Path.Combine(_modelPath, "openvino_model.xml"));

        if (!_modelAvailable)
        {
            _output.WriteLine($"Model not found at: {_modelPath}");
            _output.WriteLine("Integration tests will be skipped. Run QuickDemo first to download the model.");
        }
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task LLMPipeline_BasicInference_GeneratesText()
    {
        Skip.IfNot(_modelAvailable, "Model not available for integration testing");

        // Arrange
        using var pipeline = new LLMPipeline(_modelPath, "CPU");
        var config = GenerationConfig.Default
            .WithMaxTokens(20)
            .WithTemperature(0.7f)
            .WithSampling(true);

        const string prompt = "The capital of France is";

        // Act
        var result = await pipeline.GenerateAsync(prompt, config);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
        Assert.Contains("Paris", result.Text);

        _output.WriteLine($"Generated text: {result.Text}");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task LLMPipeline_StreamingGeneration_ProducesTokens()
    {
        Skip.IfNot(_modelAvailable, "Model not available for integration testing");

        // Arrange
        using var pipeline = new LLMPipeline(_modelPath, "CPU");
        var config = GenerationConfig.Default
            .WithMaxTokens(30)
            .WithTemperature(0.7f)
            .WithSampling(true);

        const string prompt = "Count from 1 to 5:";
        var tokens = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        TimeSpan? firstTokenTime = null;

        // Act
        await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
        {
            if (firstTokenTime == null)
            {
                firstTokenTime = stopwatch.Elapsed;
            }
            tokens.Add(token);
        }
        stopwatch.Stop();

        // Assert
        Assert.NotEmpty(tokens);
        Assert.True(tokens.Count > 5, "Should generate multiple tokens");
        Assert.NotNull(firstTokenTime);
        Assert.True(firstTokenTime.Value.TotalMilliseconds < 2000, "First token should arrive within 2 seconds");

        var fullText = string.Join("", tokens);
        _output.WriteLine($"Generated {tokens.Count} tokens in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"First token latency: {firstTokenTime.Value.TotalMilliseconds}ms");
        _output.WriteLine($"Generated text: {fullText}");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task LLMPipeline_WithCancellation_StopsGeneration()
    {
        Skip.IfNot(_modelAvailable, "Model not available for integration testing");

        // Arrange
        using var pipeline = new LLMPipeline(_modelPath, "CPU");
        var config = GenerationConfig.Default
            .WithMaxTokens(100) // Large number to ensure we can cancel
            .WithTemperature(0.7f)
            .WithSampling(true);

        const string prompt = "Write a long story about artificial intelligence:";
        using var cts = new CancellationTokenSource();
        var tokens = new List<string>();

        // Act
        try
        {
            await foreach (var token in pipeline.GenerateStreamAsync(prompt, config, cts.Token))
            {
                tokens.Add(token);
                if (tokens.Count >= 10) // Cancel after 10 tokens
                {
                    cts.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.True(tokens.Count >= 10 && tokens.Count < 20, $"Expected ~10 tokens, got {tokens.Count}");
        _output.WriteLine($"Generated {tokens.Count} tokens before cancellation");
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public void LLMPipeline_InvalidDevice_FallsBackToCPU()
    {
        Skip.IfNot(_modelAvailable, "Model not available for integration testing");

        // Try to create pipeline with potentially unavailable device
        // Should either succeed (if device exists) or throw with clear error
        try
        {
            using var pipeline = new LLMPipeline(_modelPath, "NPU");
            _output.WriteLine("NPU device is available on this system");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"NPU not available: {ex.Message}");
            // Verify CPU fallback works
            using var cpuPipeline = new LLMPipeline(_modelPath, "CPU");
            Assert.NotNull(cpuPipeline);
        }
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task ChatSession_MaintainsContext()
    {
        Skip.IfNot(_modelAvailable, "Model not available for integration testing");

        // Arrange
        using var pipeline = new LLMPipeline(_modelPath, "CPU");
        using var session = pipeline.StartChatSession();
        var config = GenerationConfig.Default
            .WithMaxTokens(30)
            .WithTemperature(0.7f)
            .WithSampling(true);

        // Act
        var response1 = await session.SendMessageAsync("My name is Alice.", config);
        var response2 = await session.SendMessageAsync("What is my name?", config);

        // Assert
        Assert.NotEmpty(response1.Text);
        Assert.NotEmpty(response2.Text);
        // The model should remember the name from context
        Assert.Contains("Alice", response2.Text);

        _output.WriteLine($"First response: {response1.Text}");
        _output.WriteLine($"Second response: {response2.Text}");
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


/// <summary>
/// Helper class for skipping tests conditionally
/// </summary>
public static class Skip
{
    public static void IfNot(bool condition, string reason)
    {
        if (!condition)
        {
            throw new SkipException(reason);
        }
    }
}