using OpenVINO.NET.GenAI.Native;

namespace OpenVINO.NET.GenAI.Exceptions;

/// <summary>
/// Exception thrown by OpenVINO GenAI operations
/// </summary>
public class OpenVINOGenAIException : Exception
{
    /// <summary>
    /// Gets the OpenVINO error code
    /// </summary>
    public ov_status_e ErrorCode { get; }

    /// <summary>
    /// Gets the native error message, if available
    /// </summary>
    public string? NativeMessage { get; }

    /// <summary>
    /// Initializes a new instance of the OpenVINOGenAIException class
    /// </summary>
    /// <param name="errorCode">The OpenVINO error code</param>
    public OpenVINOGenAIException(ov_status_e errorCode)
        : base(GetDefaultMessage(errorCode))
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the OpenVINOGenAIException class
    /// </summary>
    /// <param name="errorCode">The OpenVINO error code</param>
    /// <param name="message">The error message</param>
    public OpenVINOGenAIException(ov_status_e errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the OpenVINOGenAIException class
    /// </summary>
    /// <param name="errorCode">The OpenVINO error code</param>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public OpenVINOGenAIException(ov_status_e errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the OpenVINOGenAIException class
    /// </summary>
    /// <param name="errorCode">The OpenVINO error code</param>
    /// <param name="message">The error message</param>
    /// <param name="nativeMessage">The native error message</param>
    public OpenVINOGenAIException(ov_status_e errorCode, string message, string? nativeMessage)
        : base(message)
    {
        ErrorCode = errorCode;
        NativeMessage = nativeMessage;
    }

    /// <summary>
    /// Throws an exception if the status indicates an error
    /// </summary>
    /// <param name="status">The status to check</param>
    /// <param name="operation">The operation that was attempted</param>
    /// <exception cref="OpenVINOGenAIException">Thrown if status indicates an error</exception>
    internal static void ThrowIfError(ov_status_e status, string operation)
    {
        if (status != ov_status_e.OK)
        {
            var message = $"OpenVINO GenAI operation '{operation}' failed with status: {status}";
            throw new OpenVINOGenAIException(status, message);
        }
    }

    /// <summary>
    /// Gets the default error message for a status code
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <returns>The default error message</returns>
    private static string GetDefaultMessage(ov_status_e errorCode)
    {
        return errorCode switch
        {
            ov_status_e.OK => "Operation completed successfully",
            ov_status_e.GENERAL_ERROR => "A general error occurred",
            ov_status_e.NOT_IMPLEMENTED => "The requested operation is not implemented",
            ov_status_e.NETWORK_NOT_LOADED => "The network is not loaded",
            ov_status_e.PARAMETER_MISMATCH => "Parameter mismatch error",
            ov_status_e.NOT_FOUND => "The requested resource was not found",
            ov_status_e.OUT_OF_BOUNDS => "Index or value is out of bounds",
            ov_status_e.UNEXPECTED => "An unexpected error occurred",
            ov_status_e.REQUEST_BUSY => "The request is busy",
            ov_status_e.RESULT_NOT_READY => "The result is not ready",
            ov_status_e.NOT_ALLOCATED => "Memory is not allocated",
            ov_status_e.INFER_NOT_STARTED => "Inference has not started",
            ov_status_e.NETWORK_NOT_READ => "The network is not read",
            ov_status_e.INFER_CANCELLED => "Inference was cancelled",
            _ => $"Unknown error occurred (code: {errorCode})"
        };
    }
}
