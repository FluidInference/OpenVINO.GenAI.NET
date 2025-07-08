using System.CommandLine;
using System.Diagnostics;
using OpenVINO.NET.GenAI;

namespace WhisperDemo;

class Program
{
    // Fixed configuration for simplicity
    private const string ModelId = "openai/whisper-tiny.en";
    private const string ModelsDirectory = "./Models";

    // Sample audio data (simulated 16kHz mono audio - sine wave for demonstration)
    private static readonly float[] SampleAudioData = GenerateSampleAudio();

    static async Task<int> Main(string[] args)
    {
        var deviceOption = new Option<string>(
            name: "--device",
            description: "Device to run inference on (CPU, GPU, NPU)",
            getDefaultValue: () => "CPU");

        var benchmarkOption = new Option<bool>(
            name: "--benchmark",
            description: "Run benchmark comparing all available devices");

        var streamingOption = new Option<bool>(
            name: "--streaming",
            description: "Demonstrate streaming transcription");

        var rootCommand = new RootCommand("OpenVINO.NET Whisper Demo - Showcase speech transcription capabilities")
        {
            deviceOption,
            benchmarkOption,
            streamingOption
        };

        rootCommand.SetHandler(async (device, benchmark, streaming) =>
        {
            await RunDemoAsync(device, benchmark, streaming);
        }, deviceOption, benchmarkOption, streamingOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunDemoAsync(string device, bool benchmark, bool streaming)
    {
        try
        {
            Console.WriteLine("OpenVINO.NET Whisper Demo");
            Console.WriteLine("=========================");
            Console.WriteLine($"Model: {ModelId.Split('/')[1]}");
            Console.WriteLine($"Audio Duration: {SampleAudioData.Length / 16000.0:F1} seconds");
            Console.WriteLine();

            // Ensure model is downloaded
            var modelPath = await EnsureModelDownloadedAsync();

            if (benchmark)
            {
                await RunBenchmarkAsync(modelPath);
            }
            else if (streaming)
            {
                await RunStreamingDemoAsync(modelPath, device);
            }
            else
            {
                await RunSingleDeviceDemoAsync(modelPath, device);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure you have the required OpenVINO runtime with Whisper support installed.");
            Environment.Exit(1);
        }
    }

    static async Task<string> EnsureModelDownloadedAsync()
    {
        // Check for environment variable override (used in CI)
        var envModelPath = Environment.GetEnvironmentVariable("WHISPERDEMO_MODEL_PATH");
        if (!string.IsNullOrEmpty(envModelPath) && Directory.Exists(envModelPath))
        {
            Console.WriteLine($"✓ Using model from environment variable: {envModelPath}");
            return envModelPath;
        }

        var modelPath = Path.Combine(ModelsDirectory, ModelId.Split('/')[1]);

        if (Directory.Exists(modelPath) && Directory.GetFiles(modelPath, "*.xml").Length > 0)
        {
            Console.WriteLine($"✓ Model found at: {modelPath}");
            return modelPath;
        }

        Console.WriteLine($"Downloading model: {ModelId}");
        Console.WriteLine("This may take a few minutes for the first run...");
        Console.WriteLine();

        await DownloadModelAsync(ModelId, modelPath);

        Console.WriteLine("✓ Model download completed!");
        Console.WriteLine();
        return modelPath;
    }

    static async Task DownloadModelAsync(string modelId, string localPath)
    {
        Directory.CreateDirectory(localPath);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenVINO.NET/1.0");

        // Whisper model files for OpenVINO
        var files = new[]
        {
            "openvino_encoder_model.xml",
            "openvino_encoder_model.bin",
            "openvino_decoder_model.xml", 
            "openvino_decoder_model.bin",
            "openvino_decoder_with_past_model.xml",
            "openvino_decoder_with_past_model.bin",
            "config.json",
            "preprocessor_config.json"
        };

        var totalFiles = files.Length;
        var completedFiles = 0;

        foreach (var file in files)
        {
            var url = $"https://huggingface.co/{modelId}/resolve/main/{file}";
            var filePath = Path.Combine(localPath, file);

            try
            {
                Console.Write($"Downloading {file}... ");

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(filePath);
                await response.Content.CopyToAsync(fileStream);

                completedFiles++;
                Console.WriteLine($"✓ ({completedFiles}/{totalFiles})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
                // Some files might be optional, continue with others
            }
        }

        Console.WriteLine();
    }

    static async Task RunSingleDeviceDemoAsync(string modelPath, string device)
    {
        Console.WriteLine($"Device: {device.ToUpper()}");
        Console.WriteLine();

        try
        {
            using var pipeline = new WhisperPipeline(modelPath, device.ToUpper());
            
            // Test different configuration scenarios
            var configs = new[]
            {
                ("English Transcription", WhisperConfig.Default.ForTranscriptionEnglish()),
                ("With Timestamps", WhisperConfig.Default.ForTranscriptionEnglish().WithTimestamps(true)),
                ("With Initial Prompt", WhisperConfig.Default.ForTranscriptionEnglish()
                    .WithInitialPrompt("This is a sample audio for testing.")
                    .WithTimestamps(true))
            };

            foreach (var (description, config) in configs)
            {
                Console.WriteLine($"Test: {description}");

                var stopwatch = Stopwatch.StartNew();
                var result = await pipeline.GenerateAsync(SampleAudioData, config);
                stopwatch.Stop();

                Console.WriteLine($"Transcription: \"{result.Text}\"");
                
                if (result.Chunks.Length > 0)
                {
                    Console.WriteLine("Timestamps:");
                    foreach (var chunk in result.Chunks)
                    {
                        Console.WriteLine($"  {chunk}");
                    }
                }

                Console.WriteLine($"Performance: {stopwatch.ElapsedMilliseconds}ms total");
                Console.WriteLine($"Features Extraction: {result.PerformanceMetrics}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run on {device}: {ex.Message}");
            if (device.ToUpper() != "CPU")
            {
                Console.WriteLine("Trying CPU fallback...");
                await RunSingleDeviceDemoAsync(modelPath, "CPU");
            }
        }
    }

    static async Task RunStreamingDemoAsync(string modelPath, string device)
    {
        Console.WriteLine($"Streaming Demo on {device.ToUpper()}");
        Console.WriteLine("===================");
        Console.WriteLine();

        try
        {
            using var pipeline = new WhisperPipeline(modelPath, device.ToUpper());
            var config = WhisperConfig.Default.ForTranscriptionEnglish().WithTimestamps(true);

            Console.WriteLine("Starting streaming transcription...");
            Console.Write("Transcription: \"");

            var chunkCount = 0;
            var stopwatch = Stopwatch.StartNew();

            await foreach (var chunk in pipeline.GenerateStreamAsync(SampleAudioData, config))
            {
                Console.Write($"[{chunk.StartTime:F1}s-{chunk.EndTime:F1}s: {chunk.Text}] ");
                chunkCount++;
            }

            stopwatch.Stop();
            Console.WriteLine("\"");
            Console.WriteLine($"Received {chunkCount} chunks in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Streaming failed on {device}: {ex.Message}");
        }
    }

    static async Task RunBenchmarkAsync(string modelPath)
    {
        Console.WriteLine("Device Benchmark Mode");
        Console.WriteLine("====================");
        Console.WriteLine();

        var devices = new[] { "CPU", "GPU", "NPU" };
        var results = new List<BenchmarkResult>();

        foreach (var device in devices)
        {
            Console.WriteLine($"Testing {device}...");

            try
            {
                using var pipeline = new WhisperPipeline(modelPath, device);
                var config = WhisperConfig.Default.ForTranscriptionEnglish();

                var deviceResults = new List<double>();

                // Run multiple iterations for average
                for (int i = 0; i < 3; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var result = await pipeline.GenerateAsync(SampleAudioData, config);
                    stopwatch.Stop();

                    var audioSeconds = SampleAudioData.Length / 16000.0;
                    var rtf = stopwatch.Elapsed.TotalSeconds / audioSeconds; // Real-time factor

                    deviceResults.Add(rtf);
                }

                var avgRtf = deviceResults.Average();

                results.Add(new BenchmarkResult
                {
                    Device = device,
                    RealTimeFactor = avgRtf,
                    Success = true
                });

                Console.WriteLine($"✓ {device}: {avgRtf:F2}x real-time");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {device}: {ex.Message}");
                results.Add(new BenchmarkResult
                {
                    Device = device,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        Console.WriteLine();
        DisplayBenchmarkResults(results);
    }

    static void DisplayBenchmarkResults(List<BenchmarkResult> results)
    {
        Console.WriteLine("Device Benchmark Results:");
        Console.WriteLine("========================");
        Console.WriteLine("Device    | Real-Time Factor | Status");
        Console.WriteLine("--------- | ---------------- | ------");

        foreach (var result in results)
        {
            if (result.Success)
            {
                var performance = result.RealTimeFactor < 1.0 ? "⚡ Fast" : "🐌 Slow";
                Console.WriteLine($"{result.Device,-9} | {result.RealTimeFactor,13:F2}x | ✓ {performance}");
            }
            else
            {
                Console.WriteLine($"{result.Device,-9} | {"N/A",16} | ✗");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Note: Real-time factor < 1.0 means faster than real-time processing");

        // Show winner
        var bestResult = results.Where(r => r.Success).OrderBy(r => r.RealTimeFactor).FirstOrDefault();
        if (bestResult != null)
        {
            Console.WriteLine($"🏆 Best Performance: {bestResult.Device} ({bestResult.RealTimeFactor:F2}x real-time)");
        }

        // Show any failures
        var failures = results.Where(r => !r.Success).ToList();
        if (failures.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Device Availability Notes:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"• {failure.Device}: {failure.ErrorMessage}");
            }
        }
    }

    static float[] GenerateSampleAudio()
    {
        // Generate 5 seconds of sample audio at 16kHz (sine wave)
        const int sampleRate = 16000;
        const int duration = 5; // seconds
        const double frequency = 440.0; // A4 note

        var samples = new float[sampleRate * duration];
        for (int i = 0; i < samples.Length; i++)
        {
            var time = (double)i / sampleRate;
            samples[i] = (float)(0.5 * Math.Sin(2 * Math.PI * frequency * time));
        }

        return samples;
    }

    class BenchmarkResult
    {
        public string Device { get; set; } = "";
        public double RealTimeFactor { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}