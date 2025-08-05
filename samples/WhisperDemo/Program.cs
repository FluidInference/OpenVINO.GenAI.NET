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

            // Check if model directory exists
            if (!Directory.Exists(modelPath))
            {
                Console.WriteLine($"Error: Model directory not found: {modelPath}");
                Environment.Exit(1);
            }

            // Check for model files
            var modelFiles = new[] { "openvino_encoder_model.xml", "openvino_decoder_model.xml" };
            foreach (var file in modelFiles)
            {
                var filePath = Path.Combine(modelPath, file);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: Required model file not found: {filePath}");
                    Environment.Exit(1);
                }
            }

            // Warn about INT4 models on CPU
            if (modelPath.Contains("int4", StringComparison.OrdinalIgnoreCase) && device == "CPU")
            {
                Console.WriteLine("WARNING: INT4 quantized models are NPU-specific and may not work on CPU.");
                Console.WriteLine("Consider using an FP16 model for CPU inference.\n");
            }

            // Create pipeline
            Console.WriteLine("Creating Whisper pipeline...");
            using var pipeline = new WhisperPipeline(modelPath, device);
            Console.WriteLine("Pipeline created successfully.");

            // Configure generation
            var config = WhisperGenerationConfig.Default
                .WithTask(WhisperTask.Transcribe);

            // Transcribe the audio file
            Console.WriteLine("Starting transcription...");
            var results = pipeline.TranscribeFile(audioFile, config);
            Console.WriteLine($"Transcription completed. Found {results.Count} result(s).\n");

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