using System;
using System.Runtime.InteropServices;

class TestWhisper
{
    [DllImport("runtimes/linux-x64/native/libopenvino_genai_c.so", CallingConvention = CallingConvention.Cdecl)]
    static extern int ov_genai_whisper_pipeline_create(
        string models_path,
        string device,
        nuint property_args_size,
        out IntPtr pipeline);

    static void Main()
    {
        Console.WriteLine("Testing Whisper pipeline creation...");
        
        string modelPath = "Models/whisper-tiny-ov-fp16";
        string device = "CPU";
        
        try
        {
            int status = ov_genai_whisper_pipeline_create(
                modelPath,
                device,
                0,
                out IntPtr pipeline);
            
            Console.WriteLine($"Status: {status}");
            Console.WriteLine($"Pipeline handle: {pipeline}");
            
            if (status == 0)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine($"Failed with status: {status}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}