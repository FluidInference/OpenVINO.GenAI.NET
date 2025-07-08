using System.Runtime.InteropServices;

namespace OpenVINO.NET.GenAI.Native;

/// <summary>
/// OpenVINO status codes enumeration
/// </summary>
public enum ov_status_e
{
    OK = 0,
    GENERAL_ERROR = -1,
    NOT_IMPLEMENTED = -2,
    NETWORK_NOT_LOADED = -3,
    PARAMETER_MISMATCH = -4,
    NOT_FOUND = -5,
    OUT_OF_BOUNDS = -6,
    UNEXPECTED = -7,
    REQUEST_BUSY = -8,
    RESULT_NOT_READY = -9,
    NOT_ALLOCATED = -10,
    INFER_NOT_STARTED = -11,
    NETWORK_NOT_READ = -12,
    INFER_CANCELLED = -13
}

/// <summary>
/// Controls the stopping condition for grouped beam search
/// </summary>
public enum StopCriteria
{
    EARLY = 0,
    HEURISTIC = 1,
    NEVER = 2
}

/// <summary>
/// Streaming status enumeration
/// </summary>
public enum ov_genai_streamming_status_e
{
    RUNNING = 0,
    STOP = 1,
    CANCEL = 2
}

/// <summary>
/// Callback function delegate for streaming generation
/// </summary>
/// <param name="str">Generated token string</param>
/// <param name="args">User-defined arguments</param>
/// <returns>Streaming status</returns>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ov_genai_streamming_status_e StreamerCallbackFunc(
    [MarshalAs(UnmanagedType.LPStr)] string str,
    IntPtr args);

/// <summary>
/// Streamer callback structure
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct streamer_callback
{
    public IntPtr callback_func;
    public IntPtr args;
}

#region Whisper-specific structures and enums

/// <summary>
/// Whisper transcription result chunk
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct whisper_result_chunk
{
    public float start_ts;
    public float end_ts;
    [MarshalAs(UnmanagedType.LPStr)]
    public string text;
}

/// <summary>
/// Whisper callback function delegate for streaming transcription
/// </summary>
/// <param name="chunk">Transcription chunk</param>
/// <param name="args">User-defined arguments</param>
/// <returns>Streaming status</returns>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ov_genai_streamming_status_e WhisperStreamingCallbackFunc(
    whisper_result_chunk chunk,
    IntPtr args);

/// <summary>
/// Whisper streaming callback structure
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct whisper_streaming_callback
{
    public IntPtr callback_func;
    public IntPtr args;
}

#endregion