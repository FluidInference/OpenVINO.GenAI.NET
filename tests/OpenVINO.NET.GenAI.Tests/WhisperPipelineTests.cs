using OpenVINO.NET.GenAI;
using OpenVINO.NET.GenAI.Exceptions;
using Xunit;

namespace OpenVINO.NET.GenAI.Tests;

/// <summary>
/// Unit tests for WhisperPipeline and related classes
/// </summary>
public class WhisperPipelineTests
{
    [Fact]
    public void WhisperGenerationConfig_DefaultCreation_Succeeds()
    {
        // Act
        using var config = new WhisperGenerationConfig();

        // Assert
        Assert.NotNull(config);
    }

    [Fact]
    public void WhisperGenerationConfig_FluentAPI_ChainsCorrectly()
    {
        // Act
        using var config = WhisperGenerationConfig.Default
            .WithTask(WhisperTask.Transcribe)
            .WithTimestamps(true);

        // Assert
        Assert.NotNull(config);
    }

    // Language configuration tests removed - not currently supported in C API

    [Fact]
    public void WhisperGenerationConfig_WithTask_AcceptsBothOptions()
    {
        // Arrange
        using var config1 = new WhisperGenerationConfig();
        using var config2 = new WhisperGenerationConfig();

        // Act
        config1.WithTask(WhisperTask.Transcribe);
        config2.WithTask(WhisperTask.Translate);

        // Assert - should not throw
        Assert.NotNull(config1);
        Assert.NotNull(config2);
    }

    [Fact]
    public void WhisperGenerationConfig_Dispose_MultipleCalls_DoesNotThrow()
    {
        // Arrange
        var config = new WhisperGenerationConfig();

        // Act
        config.Dispose();
        config.Dispose(); // Second dispose should not throw

        // Assert - no exception
    }

    [Fact]
    public void AudioUtils_FromPcm16_ConvertsCorrectly()
    {
        // Arrange
        byte[] pcmData = new byte[] { 0x00, 0x00, 0xFF, 0x7F, 0x00, 0x80 }; // 0, 32767, -32768

        // Act
        var floatData = AudioUtils.FromPcm16(pcmData);

        // Assert
        Assert.Equal(3, floatData.Length);
        Assert.Equal(0.0f, floatData[0], 0.001f);
        Assert.True(floatData[1] > 0.99f && floatData[1] <= 1.0f); // Close to 1.0
        Assert.True(floatData[2] < -0.99f && floatData[2] >= -1.0f); // Close to -1.0
    }

    [Fact]
    public void AudioUtils_FromPcm16_InvalidLength_Throws()
    {
        // Arrange
        byte[] oddLengthData = new byte[] { 0x00, 0x00, 0xFF }; // Odd number of bytes

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AudioUtils.FromPcm16(oddLengthData));
    }

    [Fact]
    public void AudioUtils_NormalizeAudio_NormalizesCorrectly()
    {
        // Arrange
        float[] audioData = new float[] { 0.5f, -0.5f, 2.0f, -2.0f };

        // Act
        var normalized = AudioUtils.NormalizeAudio(audioData);

        // Assert
        Assert.Equal(4, normalized.Length);
        Assert.Equal(0.25f, normalized[0], 0.001f);
        Assert.Equal(-0.25f, normalized[1], 0.001f);
        Assert.Equal(1.0f, normalized[2], 0.001f);
        Assert.Equal(-1.0f, normalized[3], 0.001f);
    }

    [Fact]
    public void AudioUtils_NormalizeAudio_AlreadyNormalized_ReturnsSame()
    {
        // Arrange
        float[] audioData = new float[] { 0.5f, -0.5f, 1.0f, -1.0f };

        // Act
        var normalized = AudioUtils.NormalizeAudio(audioData);

        // Assert
        Assert.Equal(audioData.Length, normalized.Length);
        Assert.Equal(0.5f, normalized[0], 0.001f);
        Assert.Equal(-0.5f, normalized[1], 0.001f);
        Assert.Equal(1.0f, normalized[2], 0.001f);
        Assert.Equal(-1.0f, normalized[3], 0.001f);
    }

    [Fact]
    public void AudioUtils_NormalizeAudio_Silence_ReturnsSilence()
    {
        // Arrange
        float[] audioData = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

        // Act
        var normalized = AudioUtils.NormalizeAudio(audioData);

        // Assert
        Assert.Equal(4, normalized.Length);
        Assert.All(normalized, sample => Assert.Equal(0.0f, sample));
    }

    [Fact]
    public void WhisperChunk_Properties_ReturnCorrectValues()
    {
        // Arrange & Act
        var chunk = new WhisperChunk(1.5f, 3.7f, "Hello world");

        // Assert
        Assert.Equal(1.5f, chunk.StartTime);
        Assert.Equal(3.7f, chunk.EndTime);
        Assert.Equal("Hello world", chunk.Text);
        Assert.Equal(2.2f, chunk.Duration, 0.1f);
    }

    [Fact]
    public void WhisperChunk_ToString_FormatsCorrectly()
    {
        // Arrange
        var chunk = new WhisperChunk(1.5f, 3.7f, "Hello world");

        // Act
        var str = chunk.ToString();

        // Assert
        Assert.Equal("[1.50s - 3.70s]: Hello world", str);
    }

    [Fact]
    public void WhisperDecodedResult_Properties_ReturnCorrectValues()
    {
        // Arrange
        var chunks = new List<WhisperChunk>
        {
            new WhisperChunk(0.0f, 1.5f, "Hello"),
            new WhisperChunk(1.5f, 3.0f, "world")
        };

        // Act
        var result = new WhisperDecodedResult("Hello world", 0.95f, chunks);

        // Assert
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95f, result.Score);
        Assert.True(result.HasChunks);
        Assert.NotNull(result.Chunks);
        Assert.Equal(2, result.Chunks.Count);
    }

    [Fact]
    public void WhisperDecodedResult_NoChunks_HasChunksReturnsFalse()
    {
        // Arrange & Act
        var result = new WhisperDecodedResult("Hello world", 0.95f);

        // Assert
        Assert.False(result.HasChunks);
        Assert.Null(result.Chunks);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperPipeline_Constructor_ValidatesModelPath(string modelPath)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WhisperPipeline(modelPath));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WhisperPipeline_Constructor_ValidatesDevice(string device)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WhisperPipeline("model", device));
    }

    [Fact]
    public void WhisperPipeline_Constructor_InvalidModelPath_ThrowsException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        var exception = Assert.Throws<OpenVINOGenAIException>(() => new WhisperPipeline(nonExistentPath, "CPU"));
        Assert.Contains("create Whisper pipeline", exception.Message);
    }

    [Fact]
    public void AudioUtils_LoadAudioFile_FileNotFound_Throws()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.wav");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => AudioUtils.LoadAudioFile(nonExistentFile));
    }

    [Fact]
    public void AudioUtils_LoadAudioFile_NonWavFile_Throws()
    {
        // Arrange
        var tempFile = Path.GetTempFileName() + ".mp3";
        File.WriteAllText(tempFile, "dummy content");

        try
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => AudioUtils.LoadAudioFile(tempFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}