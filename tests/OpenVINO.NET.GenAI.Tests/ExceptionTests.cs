using OpenVINO.NET.GenAI.Exceptions;
using OpenVINO.NET.GenAI.Native;
using Xunit;

namespace OpenVINO.NET.GenAI.Tests;

public class ExceptionTests
{
    [Fact]
    public void OpenVINOGenAIException_WithErrorCode_SetsProperties()
    {
        // Arrange
        const ov_status_e errorCode = ov_status_e.GENERAL_ERROR;

        // Act
        var exception = new OpenVINOGenAIException(errorCode);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Contains("general error", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OpenVINOGenAIException_WithCustomMessage_SetsMessage()
    {
        // Arrange
        const ov_status_e errorCode = ov_status_e.NOT_FOUND;
        const string customMessage = "Custom error message";

        // Act
        var exception = new OpenVINOGenAIException(errorCode, customMessage);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsProperties()
    {
        // Arrange
        const ov_status_e errorCode = ov_status_e.GENERAL_ERROR;
        const string message = "Test message";
        var innerException = new InvalidOperationException("Inner message");

        // Act
        var exception = new OpenVINOGenAIException(errorCode, message, innerException);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithNativeMessage_SetsNativeMessage()
    {
        // Arrange
        const ov_status_e errorCode = ov_status_e.GENERAL_ERROR;
        const string message = "Test message";
        const string nativeMessage = "Native error details";

        // Act
        var exception = new OpenVINOGenAIException(errorCode, message, nativeMessage);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
        Assert.Equal(nativeMessage, exception.NativeMessage);
    }

    [Theory]
    [InlineData(ov_status_e.OK, "Operation completed successfully")]
    [InlineData(ov_status_e.GENERAL_ERROR, "A general error occurred")]
    [InlineData(ov_status_e.NOT_IMPLEMENTED, "The requested operation is not implemented")]
    [InlineData(ov_status_e.NETWORK_NOT_LOADED, "The network is not loaded")]
    [InlineData(ov_status_e.PARAMETER_MISMATCH, "Parameter mismatch error")]
    [InlineData(ov_status_e.NOT_FOUND, "The requested resource was not found")]
    [InlineData(ov_status_e.OUT_OF_BOUNDS, "Index or value is out of bounds")]
    [InlineData(ov_status_e.UNEXPECTED, "An unexpected error occurred")]
    public void DefaultErrorMessages_AreCorrect(ov_status_e errorCode, string expectedMessage)
    {
        // Act
        var exception = new OpenVINOGenAIException(errorCode);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }
}
