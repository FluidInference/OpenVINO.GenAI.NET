using System.Runtime.InteropServices;
using OpenVINO.NET.GenAI.Native;

namespace OpenVINO.NET.GenAI.SafeHandles;

/// <summary>
/// Safe handle for LLM Pipeline native resources
/// </summary>
public sealed class LLMPipelineSafeHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the LLMPipelineSafeHandle class
    /// </summary>
    public LLMPipelineSafeHandle() : base(IntPtr.Zero, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LLMPipelineSafeHandle class with an existing handle
    /// </summary>
    /// <param name="handle">The existing handle</param>
    /// <param name="ownsHandle">Whether this instance owns the handle</param>
    public LLMPipelineSafeHandle(IntPtr handle, bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
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
            GenAINativeMethods.ov_genai_llm_pipeline_free(handle);
            return true;
        }
        return false;
    }
}