using System.Diagnostics;
using OpenVINO.NET.GenAI;

namespace WhisperDemo;

class Program
{
    // Model path - will be downloaded if not present
    private const string ModelPath = "models/whisper-tiny-en";
    private const string ModelUrl = "https://huggingface.co/openai/whisper-tiny.en"; // Example URL
    
    // Sample audio file paths
    private static readonly string[] SampleAudioFiles = 
    {
        "samples/audio/sample1.wav",
        "samples/audio/sample2.wav",
        "samples/audio/sample3.wav"
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET Whisper Demo");
        Console.WriteLine("=========================\n");

        // Parse command line arguments
        var device = args.FirstOrDefault(a => a.StartsWith("--device="))?.Split('=')[1] ?? "CPU";
        var benchmark = args.Contains("--benchmark");
        var audioFile = args.FirstOrDefault(a => a.StartsWith("--audio="))?.Split('=')[1];
        var timestamps = args.Contains("--timestamps");
        var language = args.FirstOrDefault(a => a.StartsWith("--language="))?.Split('=')[1] ?? "en";
        var task = args.Contains("--translate") ? WhisperTask.Translate : WhisperTask.Transcribe;

        try
        {
            // Ensure model is available
            await EnsureModelAvailable();

            if (benchmark)
            {
                await RunBenchmark(timestamps);
            }
            else if (!string.IsNullOrEmpty(audioFile))
            {
                await TranscribeFile(audioFile, device, language, task, timestamps);
            }
            else
            {
                await RunInteractiveMode(device, language, task, timestamps);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static Task EnsureModelAvailable()
    {
        if (!Directory.Exists(ModelPath))
        {
            Console.WriteLine($"Model not found at {ModelPath}");
            Console.WriteLine("Please download a Whisper model converted for OpenVINO.");
            Console.WriteLine("\nExample models:");
            Console.WriteLine("- openai/whisper-tiny.en");
            Console.WriteLine("- openai/whisper-base.en");
            Console.WriteLine("- openai/whisper-small.en");
            Console.WriteLine("\nYou can convert models using OpenVINO Model Converter.");
            throw new InvalidOperationException("Model not found");
        }
        return Task.CompletedTask;
    }

    static Task RunBenchmark(bool includeTimestamps)
    {
        Console.WriteLine("Running Whisper benchmark...\n");

        var devices = new[] { "CPU", "GPU", "NPU" };
        var results = new List<(string device, double avgTime, double tokensPerSec)>();

        // Create a test audio sample (30 seconds of silence for consistent testing)
        var testAudio = new float[16000 * 30]; // 30 seconds at 16kHz

        foreach (var device in devices)
        {
            try
            {
                Console.WriteLine($"Testing {device}...");
                
                using var pipeline = new WhisperPipeline(ModelPath, device);
                var config = WhisperGenerationConfig.Default
                    .WithLanguage("en")
                    .WithTask(WhisperTask.Transcribe)
                    .WithTimestamps(includeTimestamps);

                // Warm-up run
                _ = pipeline.Generate(testAudio, config);

                // Benchmark runs
                var times = new List<double>();
                const int runs = 5;

                for (int i = 0; i < runs; i++)
                {
                    var sw = Stopwatch.StartNew();
                    var result = pipeline.Generate(testAudio, config);
                    sw.Stop();
                    times.Add(sw.Elapsed.TotalMilliseconds);
                    
                    Console.WriteLine($"  Run {i + 1}: {sw.Elapsed.TotalMilliseconds:F2}ms");
                }

                var avgTime = times.Average();
                var audioLengthSec = testAudio.Length / 16000.0;
                var rtf = avgTime / 1000.0 / audioLengthSec; // Real-time factor

                results.Add((device, avgTime, 1.0 / rtf));
                Console.WriteLine($"  Average: {avgTime:F2}ms (RTF: {rtf:F3})\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Failed: {ex.Message}\n");
            }
        }

        // Display results
        Console.WriteLine("\nBenchmark Results:");
        Console.WriteLine("==================");
        foreach (var (device, avgTime, speed) in results.OrderBy(r => r.avgTime))
        {
            Console.WriteLine($"{device,-10} Avg: {avgTime,8:F2}ms  Speed: {speed,6:F2}x realtime");
        }
        
        return Task.CompletedTask;
    }

    static async Task TranscribeFile(string audioFile, string device, string language, WhisperTask task, bool includeTimestamps)
    {
        if (!File.Exists(audioFile))
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile}");
        }

        Console.WriteLine($"Transcribing: {audioFile}");
        Console.WriteLine($"Device: {device}");
        Console.WriteLine($"Language: {language}");
        Console.WriteLine($"Task: {task}");
        Console.WriteLine($"Timestamps: {(includeTimestamps ? "Yes" : "No")}");
        Console.WriteLine();

        using var pipeline = new WhisperPipeline(ModelPath, device);
        
        var config = WhisperGenerationConfig.Default
            .WithLanguage(language)
            .WithTask(task)
            .WithTimestamps(includeTimestamps);

        var sw = Stopwatch.StartNew();
        var results = await pipeline.TranscribeFileAsync(audioFile, config);
        sw.Stop();

        Console.WriteLine($"Transcription completed in {sw.Elapsed.TotalSeconds:F2} seconds\n");

        foreach (var result in results)
        {
            Console.WriteLine($"Text: {result.Text}");
            Console.WriteLine($"Score: {result.Score:F4}");
            
            if (result.HasChunks && result.Chunks != null)
            {
                Console.WriteLine("\nTimestamped chunks:");
                foreach (var chunk in result.Chunks)
                {
                    Console.WriteLine($"  {chunk}");
                }
            }
            Console.WriteLine();
        }
    }

    static async Task RunInteractiveMode(string device, string language, WhisperTask task, bool includeTimestamps)
    {
        Console.WriteLine("Interactive Whisper Demo");
        Console.WriteLine($"Device: {device}");
        Console.WriteLine($"Language: {language}");
        Console.WriteLine($"Task: {task}");
        Console.WriteLine($"Timestamps: {(includeTimestamps ? "Yes" : "No")}");
        Console.WriteLine("\nCommands:");
        Console.WriteLine("  <audio_file_path> - Transcribe an audio file");
        Console.WriteLine("  benchmark - Run performance benchmark");
        Console.WriteLine("  exit - Quit the application");
        Console.WriteLine();

        using var pipeline = new WhisperPipeline(ModelPath, device);
        
        var config = WhisperGenerationConfig.Default
            .WithLanguage(language)
            .WithTask(task)
            .WithTimestamps(includeTimestamps);

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input))
                continue;

            if (input.ToLower() == "exit")
                break;

            if (input.ToLower() == "benchmark")
            {
                await RunBenchmark(includeTimestamps);
                continue;
            }

            try
            {
                if (!File.Exists(input))
                {
                    Console.WriteLine($"File not found: {input}");
                    continue;
                }

                var sw = Stopwatch.StartNew();
                var results = await pipeline.TranscribeFileAsync(input, config);
                sw.Stop();

                Console.WriteLine($"\nTranscription ({sw.Elapsed.TotalSeconds:F2}s):");
                foreach (var result in results)
                {
                    Console.WriteLine(result.Text);
                    
                    if (result.HasChunks && result.Chunks != null)
                    {
                        Console.WriteLine("\nChunks:");
                        foreach (var chunk in result.Chunks)
                        {
                            Console.WriteLine($"  {chunk}");
                        }
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("\nGoodbye!");
    }
}