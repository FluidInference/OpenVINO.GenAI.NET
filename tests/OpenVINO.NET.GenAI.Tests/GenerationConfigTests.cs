using Xunit;
using OpenVINO.NET.GenAI;

namespace OpenVINO.NET.GenAI.Tests;

public class GenerationConfigTests
{
    [Fact]
    public void Constructor_CreatesValidConfig()
    {
        // Act
        using var config = new GenerationConfig();

        // Assert - Should not throw
        Assert.NotNull(config);
    }

    [Fact]
    public void WithMaxTokens_SetsValue()
    {
        // Arrange
        using var config = new GenerationConfig();
        const int expectedTokens = 100;

        // Act
        config.WithMaxTokens(expectedTokens);

        // Assert
        Assert.Equal(expectedTokens, config.GetMaxNewTokens());
    }

    [Fact]
    public void WithMaxTokens_NegativeValue_ThrowsException()
    {
        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithMaxTokens(-1));
    }

    [Fact]
    public void WithTemperature_NegativeValue_ThrowsException()
    {
        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTemperature(-0.1f));
    }

    [Fact]
    public void WithTopP_InvalidRange_ThrowsException()
    {
        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTopP(-0.1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTopP(1.1f));
    }

    [Fact]
    public void WithTopP_ValidRange_DoesNotThrow()
    {
        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert - Should not throw
        config.WithTopP(0.0f);
        config.WithTopP(0.5f);
        config.WithTopP(1.0f);
    }

    [Fact]
    public void FluentAPI_CanChainMethods()
    {
        // Arrange & Act
        using var config = GenerationConfig.Default
            .WithMaxTokens(150)
            .WithTemperature(0.8f)
            .WithTopP(0.9f)
            .WithTopK(50)
            .WithSampling(true);

        // Assert
        Assert.Equal(150, config.GetMaxNewTokens());
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var config = new GenerationConfig();

        // Act & Assert - Should not throw
        config.Dispose();
        config.Dispose();
    }

    [Fact]
    public void AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var config = new GenerationConfig();
        config.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => config.WithMaxTokens(100));
        Assert.Throws<ObjectDisposedException>(() => config.GetMaxNewTokens());
        Assert.Throws<ObjectDisposedException>(() => config.Validate());
    }
}