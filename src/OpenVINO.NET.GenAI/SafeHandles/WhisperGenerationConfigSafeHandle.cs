using System.Runtime.InteropServices;
using Fluid.OpenVINO.GenAI.Native;

namespace Fluid.OpenVINO.GenAI.SafeHandles;

/// <summary>
/// Safe handle for Whisper Generation Config native resources
/// </summary>
public sealed class WhisperGenerationConfigSafeHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the WhisperGenerationConfigSafeHandle class
    /// </summary>
    public WhisperGenerationConfigSafeHandle() : base(IntPtr.Zero, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the WhisperGenerationConfigSafeHandle class with an existing handle
    /// </summary>
    /// <param name="handle">The existing handle</param>
    /// <param name="ownsHandle">Whether this instance owns the handle</param>
    public WhisperGenerationConfigSafeHandle(IntPtr handle, bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// Gets a value indicating whether the handle value is invalid
    /// </summary>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Releases the native handle
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            GenAINativeMethods.ov_genai_whisper_generation_config_free(handle);
            return true;
        }
        return false;
    }
}