using OpenVINO.NET.GenAI;
using Xunit;

namespace OpenVINO.NET.GenAI.Tests;

public class GenerationConfigTests
{
    private static bool IsNativeLibraryAvailable()
    {
        try
        {
            using var config = new GenerationConfig();
            return true;
        }
        catch (System.DllNotFoundException)
        {
            return false;
        }
    }

    [SkippableFact]
    public void Constructor_CreatesValidConfig()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Act
        using var config = new GenerationConfig();

        // Assert - Should not throw
        Assert.NotNull(config);
    }

    [SkippableFact]
    public void WithMaxTokens_SetsValue()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        using var config = new GenerationConfig();
        const int expectedTokens = 100;

        // Act
        config.WithMaxTokens(expectedTokens);

        // Assert
        Assert.Equal(expectedTokens, config.GetMaxNewTokens());
    }

    [SkippableFact]
    public void WithMaxTokens_NegativeValue_ThrowsException()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithMaxTokens(-1));
    }

    [SkippableFact]
    public void WithTemperature_NegativeValue_ThrowsException()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTemperature(-0.1f));
    }

    [SkippableFact]
    public void WithTopP_InvalidRange_ThrowsException()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTopP(-0.1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithTopP(1.1f));
    }

    [SkippableFact]
    public void WithTopP_ValidRange_DoesNotThrow()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        using var config = new GenerationConfig();

        // Act & Assert - Should not throw
        config.WithTopP(0.0f);
        config.WithTopP(0.5f);
        config.WithTopP(1.0f);
    }

    [SkippableFact]
    public void FluentAPI_CanChainMethods()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

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

    [SkippableFact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        var config = new GenerationConfig();

        // Act & Assert - Should not throw
        config.Dispose();
        config.Dispose();
    }

    [SkippableFact]
    public void AfterDispose_ThrowsObjectDisposedException()
    {
        Skip.IfNot(IsNativeLibraryAvailable(), "Native OpenVINO library not available");

        // Arrange
        var config = new GenerationConfig();
        config.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => config.WithMaxTokens(100));
        Assert.Throws<ObjectDisposedException>(() => config.GetMaxNewTokens());
        Assert.Throws<ObjectDisposedException>(() => config.Validate());
    }
}
