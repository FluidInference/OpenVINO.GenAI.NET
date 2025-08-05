using OpenVINO.NET.GenAI;

namespace WhisperDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET Whisper Demo");
        Console.WriteLine("=========================\n");
        
        // Parse simple arguments: model path and audio file
        string modelPath = args.Length > 0 ? args[0] : 
            (Environment.GetEnvironmentVariable("WHISPER_MODEL_PATH") ?? "models/whisper-tiny-en");
        
        string audioFile = args.Length > 1 ? args[1] : "samples/audio/startup.wav";
        
        string device = "CPU";
        
        try
        {
            Console.WriteLine($"Model: {modelPath}");
            Console.WriteLine($"Audio: {audioFile}");
            Console.WriteLine($"Device: {device}\n");
            
            // Create pipeline
            using var pipeline = new WhisperPipeline(modelPath, device);
            
            // Configure generation
            var config = WhisperGenerationConfig.Default
                .WithTask(WhisperTask.Transcribe);
            
            // Transcribe the audio file
            var results = pipeline.TranscribeFile(audioFile, config);
            
            // Display results
            Console.WriteLine("Transcription:");
            foreach (var result in results)
            {
                Console.WriteLine($"  {result.Text}");
                Console.WriteLine($"  Score: {result.Score:F4}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}