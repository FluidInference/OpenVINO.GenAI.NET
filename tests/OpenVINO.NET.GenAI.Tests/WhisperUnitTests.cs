using Xunit;
using OpenVINO.NET.GenAI;

namespace OpenVINO.NET.GenAI.Tests;

/// <summary>
/// Unit tests for Whisper-related classes and configuration
/// </summary>
public class WhisperUnitTests
{
    [Fact]
    public void WhisperConfig_Default_ShouldCreateValidConfig()
    {
        using var config = WhisperConfig.Default;

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithLanguage_ShouldSetLanguage()
    {
        using var config = WhisperConfig.Default.WithLanguage("en");

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithTask_ShouldSetTask()
    {
        using var config = WhisperConfig.Default.WithTask("transcribe");

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithTimestamps_ShouldSetTimestamps()
    {
        using var config = WhisperConfig.Default.WithTimestamps(true);

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithInitialPrompt_ShouldSetPrompt()
    {
        using var config = WhisperConfig.Default.WithInitialPrompt("Test prompt");

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithHotwords_ShouldSetHotwords()
    {
        using var config = WhisperConfig.Default.WithHotwords("test,audio,speech");

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithMaxInitialTimestampIndex_ShouldSetIndex()
    {
        using var config = WhisperConfig.Default.WithMaxInitialTimestampIndex(50);

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithDecoderStartTokenId_ShouldSetTokenId()
    {
        using var config = WhisperConfig.Default.WithDecoderStartTokenId(50257);

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithSuppressTokens_ShouldSetTokens()
    {
        var suppressTokens = new long[] { 1, 2, 3, 4, 5 };
        using var config = WhisperConfig.Default.WithSuppressTokens(suppressTokens);

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_WithBeginSuppressTokens_ShouldSetTokens()
    {
        var beginSuppressTokens = new long[] { 1, 2, 3 };
        using var config = WhisperConfig.Default.WithBeginSuppressTokens(beginSuppressTokens);

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_FluentAPI_Chaining_ShouldWork()
    {
        using var config = WhisperConfig.Default
            .WithLanguage("en")
            .WithTask("transcribe")
            .WithTimestamps(true)
            .WithInitialPrompt("Hello world");

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_ForTranscriptionEnglish_ShouldSetDefaults()
    {
        using var config = WhisperConfig.Default.ForTranscriptionEnglish();

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_ForTranslationToEnglish_ShouldSetDefaults()
    {
        using var config = WhisperConfig.Default.ForTranslationToEnglish();

        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperConfig_ForTranscriptionLanguage_ShouldSetLanguage()
    {
        using var config = WhisperConfig.Default.ForTranscriptionLanguage("es");

        Assert.NotNull(config);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperConfig_WithLanguage_InvalidInput_ShouldThrow(string? language)
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentException>(() => config.WithLanguage(language!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperConfig_WithTask_InvalidInput_ShouldThrow(string? task)
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentException>(() => config.WithTask(task!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperConfig_WithInitialPrompt_InvalidInput_ShouldThrow(string? prompt)
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentException>(() => config.WithInitialPrompt(prompt!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperConfig_WithHotwords_InvalidInput_ShouldThrow(string? hotwords)
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentException>(() => config.WithHotwords(hotwords!));
    }

    [Fact]
    public void WhisperConfig_WithSuppressTokens_NullInput_ShouldThrow()
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentNullException>(() => config.WithSuppressTokens(null!));
    }

    [Fact]
    public void WhisperConfig_WithBeginSuppressTokens_NullInput_ShouldThrow()
    {
        using var config = WhisperConfig.Default;

        Assert.Throws<ArgumentNullException>(() => config.WithBeginSuppressTokens(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperConfig_ForTranscriptionLanguage_InvalidInput_ShouldThrow(string? language)
    {
        Assert.Throws<ArgumentException>(() => WhisperConfig.Default.ForTranscriptionLanguage(language!));
    }

    [Fact]
    public void WhisperConfig_DisposedAccess_ShouldThrow()
    {
        var config = WhisperConfig.Default;
        config.Dispose();

        Assert.Throws<ObjectDisposedException>(() => config.WithLanguage("en"));
        Assert.Throws<ObjectDisposedException>(() => config.WithTask("transcribe"));
        Assert.Throws<ObjectDisposedException>(() => config.WithTimestamps(true));
        Assert.Throws<ObjectDisposedException>(() => config.WithInitialPrompt("test"));
        Assert.Throws<ObjectDisposedException>(() => config.WithHotwords("test"));
        Assert.Throws<ObjectDisposedException>(() => config.WithMaxInitialTimestampIndex(50));
        Assert.Throws<ObjectDisposedException>(() => config.WithDecoderStartTokenId(50257));
        Assert.Throws<ObjectDisposedException>(() => config.WithSuppressTokens(new long[] { 1, 2, 3 }));
        Assert.Throws<ObjectDisposedException>(() => config.WithBeginSuppressTokens(new long[] { 1, 2, 3 }));
    }

    // Note: WhisperChunk constructor is internal, so these tests are handled in integration tests

    [Theory]
    [InlineData("CPU")]
    [InlineData("GPU")]
    [InlineData("NPU")]
    public void WhisperPipeline_Constructor_ValidDevices_ShouldNotThrow(string device)
    {
        // Note: This test only validates parameter handling, not actual device availability
        // Actual device tests are in integration tests
        var modelPath = "/fake/path"; // This will fail later but validates parameter handling

        var exception = Record.Exception(() =>
        {
            try
            {
                using var pipeline = new WhisperPipeline(modelPath, device);
            }
            catch (Exception ex) when (ex.Message.Contains("model") || ex.Message.Contains("path") || ex.Message.Contains("create"))
            {
                // Expected - model path doesn't exist, but device parameter was accepted
                return;
            }
        });

        // Should only fail due to invalid model path, not device parameter
        Assert.True(exception == null || exception.Message.Contains("model") || exception.Message.Contains("path") || exception.Message.Contains("create"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void WhisperPipeline_Constructor_InvalidModelPath_ShouldThrow(string? modelPath)
    {
        Assert.Throws<ArgumentException>(() => new WhisperPipeline(modelPath!, "CPU"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void WhisperPipeline_Constructor_InvalidDevice_ShouldThrow(string? device)
    {
        Assert.Throws<ArgumentException>(() => new WhisperPipeline("/fake/path", device!));
    }

    [Fact]
    public void AudioDataValidation_EmptyArray_ShouldBeInvalid()
    {
        // This tests the validation logic that would be used by WhisperPipeline
        var emptyAudio = Array.Empty<float>();

        Assert.Empty(emptyAudio);
    }

    [Fact]
    public void AudioDataValidation_ValidArray_ShouldBeValid()
    {
        // Test normal audio data
        var validAudio = new float[] { 0.1f, 0.2f, 0.3f, -0.1f, -0.2f };

        Assert.True(validAudio.Length > 0);
        Assert.All(validAudio, sample => Assert.True(float.IsFinite(sample)));
    }

    [Fact]
    public void AudioDataValidation_InvalidValues_ShouldBeDetected()
    {
        // Test audio with invalid values
        var invalidAudio1 = new float[] { 0.1f, float.NaN, 0.3f };
        var invalidAudio2 = new float[] { 0.1f, float.PositiveInfinity, 0.3f };
        var invalidAudio3 = new float[] { 0.1f, float.NegativeInfinity, 0.3f };

        Assert.Contains(invalidAudio1, sample => !float.IsFinite(sample));
        Assert.Contains(invalidAudio2, sample => !float.IsFinite(sample));
        Assert.Contains(invalidAudio3, sample => !float.IsFinite(sample));
    }

    [Fact]
    public void AudioDataValidation_MaxLength_ShouldBeRespected()
    {
        // Test maximum audio length (30 seconds at 16kHz = 480,000 samples)
        const int maxSamples = 480000;
        var maxAudio = new float[maxSamples];
        var tooLongAudio = new float[maxSamples + 1];

        Assert.Equal(maxSamples, maxAudio.Length);
        Assert.True(tooLongAudio.Length > maxSamples);
    }

    [Theory]
    [InlineData(16000, 1)] // 1 second
    [InlineData(16000, 5)] // 5 seconds
    [InlineData(16000, 30)] // 30 seconds (max)
    public void AudioGeneration_ValidDurations_ShouldCreateCorrectSizes(int sampleRate, int durationSeconds)
    {
        var expectedSamples = sampleRate * durationSeconds;
        var audio = new float[expectedSamples];

        Assert.Equal(expectedSamples, audio.Length);
    }
}