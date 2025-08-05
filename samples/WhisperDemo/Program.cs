using System.Diagnostics;
using OpenVINO.NET.GenAI;

namespace WhisperDemo;

class Program
{
    // Model path - will be downloaded if not present
    private static readonly string ModelPath = Path.GetFullPath("models/whisper-tiny-en");
    private const string ModelUrl = "https://huggingface.co/openai/whisper-tiny.en"; // Example URL

    // Sample audio file paths
    private static readonly string[] SampleAudioFiles =
    {
        "samples/audio/glm.wav",
        "samples/audio/startup.wav"
    };

    static async Task Main(string[] args)
    {
        // Parse command line arguments
        var device = args.FirstOrDefault(a => a.StartsWith("--device="))?.Split('=')[1] ?? "CPU";
        var benchmark = args.Contains("--benchmark");
        var audioFile = args.FirstOrDefault(a => a.StartsWith("--audio="))?.Split('=')[1];
        var audioDir = args.FirstOrDefault(a => a.StartsWith("--audio-dir="))?.Split('=')[1];
        var timestamps = args.Contains("--timestamps");
        // Language parameter removed - not currently supported in the C API
        var task = args.Contains("--translate") ? WhisperTask.Translate : WhisperTask.Transcribe;
        var workflow = args.Contains("--workflow");

        if (!workflow)
        {
            Console.WriteLine("OpenVINO.NET Whisper Demo");
            Console.WriteLine("=========================\n");
        }

        try
        {
            // Ensure model is available
            await EnsureModelAvailable();

            if (benchmark)
            {
                await RunBenchmark(timestamps);
            }
            else if (!string.IsNullOrEmpty(audioDir))
            {
                await TranscribeDirectory(audioDir, device, task, timestamps, workflow);
            }
            else if (!string.IsNullOrEmpty(audioFile))
            {
                await TranscribeFile(audioFile, device, task, timestamps);
            }
            else if (workflow)
            {
                // In workflow mode without specific audio, create test audio
                await RunWorkflowTest(device, task, timestamps);
            }
            else
            {
                await RunInteractiveMode(device, task, timestamps);
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

    static async Task TranscribeFile(string audioFile, string device, WhisperTask task, bool includeTimestamps)
    {
        if (!File.Exists(audioFile))
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile}");
        }

        Console.WriteLine($"Transcribing: {audioFile}");
        Console.WriteLine($"Device: {device}");
        Console.WriteLine($"Task: {task}");
        Console.WriteLine($"Timestamps: {(includeTimestamps ? "Yes" : "No")}");
        Console.WriteLine();

        using var pipeline = new WhisperPipeline(ModelPath, device);

        var config = WhisperGenerationConfig.Default
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

    static async Task RunInteractiveMode(string device, WhisperTask task, bool includeTimestamps)
    {
        Console.WriteLine("Interactive Whisper Demo");
        Console.WriteLine($"Device: {device}");
        Console.WriteLine($"Task: {task}");
        Console.WriteLine($"Timestamps: {(includeTimestamps ? "Yes" : "No")}");
        Console.WriteLine("\nCommands:");
        Console.WriteLine("  <audio_file_path> - Transcribe an audio file");
        Console.WriteLine("  benchmark - Run performance benchmark");
        Console.WriteLine("  exit - Quit the application");
        Console.WriteLine();

        using var pipeline = new WhisperPipeline(ModelPath, device);

        var config = WhisperGenerationConfig.Default
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

    static async Task TranscribeDirectory(string directory, string device, WhisperTask task, bool includeTimestamps, bool workflow)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Audio directory not found: {directory}");
        }

        var audioFiles = Directory.GetFiles(directory, "*.wav")
            .OrderBy(f => f)
            .ToArray();

        if (audioFiles.Length == 0)
        {
            Console.WriteLine($"No WAV files found in: {directory}");
            return;
        }

        if (!workflow)
        {
            Console.WriteLine($"Found {audioFiles.Length} audio files in: {directory}");
            Console.WriteLine($"Device: {device}");
                Console.WriteLine($"Task: {task}");
            Console.WriteLine($"Timestamps: {(includeTimestamps ? "Yes" : "No")}");
            Console.WriteLine();
        }

        using var pipeline = new WhisperPipeline(ModelPath, device);
        
        var config = WhisperGenerationConfig.Default
            .WithTask(task)
            .WithTimestamps(includeTimestamps);

        var totalAudioDuration = 0.0;
        var totalProcessingTime = 0.0;

        foreach (var audioFile in audioFiles)
        {
            var fileName = Path.GetFileName(audioFile);
            
            if (!workflow)
            {
                Console.WriteLine($"Processing: {fileName}");
            }

            var sw = Stopwatch.StartNew();
            var results = await pipeline.TranscribeFileAsync(audioFile, config);
            sw.Stop();

            totalProcessingTime += sw.Elapsed.TotalSeconds;

            // Estimate audio duration (rough calculation based on file size)
            var fileInfo = new FileInfo(audioFile);
            var estimatedDuration = fileInfo.Length / 32000.0; // Assuming 16kHz, 16-bit mono
            totalAudioDuration += estimatedDuration;

            if (workflow)
            {
                // Workflow output format
                Console.WriteLine($"### {fileName}");
                foreach (var result in results)
                {
                    Console.WriteLine($"Text: {result.Text}");
                    Console.WriteLine($"Score: {result.Score:F4}");
                    
                    if (result.HasChunks && result.Chunks != null)
                    {
                        Console.WriteLine("Timestamps:");
                        foreach (var chunk in result.Chunks)
                        {
                            Console.WriteLine($"  {chunk}");
                        }
                    }
                }
                Console.WriteLine($"Processing time: {sw.Elapsed.TotalSeconds:F2}s");
                Console.WriteLine();
            }
            else
            {
                // Interactive output format
                foreach (var result in results)
                {
                    Console.WriteLine($"  Text: {result.Text}");
                    Console.WriteLine($"  Score: {result.Score:F4}");
                    
                    if (result.HasChunks && result.Chunks != null)
                    {
                        Console.WriteLine("  Chunks:");
                        foreach (var chunk in result.Chunks)
                        {
                            Console.WriteLine($"    {chunk}");
                        }
                    }
                }
                Console.WriteLine($"  Time: {sw.Elapsed.TotalSeconds:F2}s");
                Console.WriteLine();
            }
        }

        // Summary
        if (audioFiles.Length > 1)
        {
            var avgSpeed = totalAudioDuration / totalProcessingTime;
            Console.WriteLine($"## Summary");
            Console.WriteLine($"Total files: {audioFiles.Length}");
            Console.WriteLine($"Total audio duration: ~{totalAudioDuration:F1}s");
            Console.WriteLine($"Total processing time: {totalProcessingTime:F2}s");
            Console.WriteLine($"Average speed: {avgSpeed:F1}x realtime");
        }
    }

    static async Task RunWorkflowTest(string device, WhisperTask task, bool includeTimestamps)
    {
        Console.WriteLine("## Workflow Test Mode");
        Console.WriteLine($"Device: {device}");
        Console.WriteLine($"Task: {task}");
        Console.WriteLine($"Timestamps: {includeTimestamps}");
        Console.WriteLine();

        using var pipeline = new WhisperPipeline(ModelPath, device);
        
        var config = WhisperGenerationConfig.Default
            .WithTask(task)
            .WithTimestamps(includeTimestamps);

        // Generate test audio
        var testAudio = GenerateTestAudio(5.0f);
        Console.WriteLine("### Generated Test Audio (5 seconds)");

        var sw = Stopwatch.StartNew();
        var results = await pipeline.GenerateAsync(testAudio, config);
        sw.Stop();

        foreach (var result in results)
        {
            Console.WriteLine($"Text: {result.Text}");
            Console.WriteLine($"Score: {result.Score:F4}");
            
            if (result.HasChunks && result.Chunks != null)
            {
                Console.WriteLine("Timestamps:");
                foreach (var chunk in result.Chunks)
                {
                    Console.WriteLine($"  {chunk}");
                }
            }
        }
        
        Console.WriteLine($"Processing time: {sw.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"Speed: {5.0 / sw.Elapsed.TotalSeconds:F1}x realtime");
    }

    /// <summary>
    /// Generates test audio data (sine wave to simulate speech patterns)
    /// </summary>
    private static float[] GenerateTestAudio(float durationSeconds)
    {
        const int sampleRate = 16000;
        var sampleCount = (int)(sampleRate * durationSeconds);
        var audio = new float[sampleCount];

        // Generate a modulated sine wave to simulate speech-like patterns
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            // Mix of frequencies to simulate speech
            float carrier = (float)Math.Sin(2 * Math.PI * 440 * t); // 440 Hz carrier
            float modulator = (float)Math.Sin(2 * Math.PI * 3 * t); // 3 Hz modulation
            audio[i] = carrier * 0.3f * (1 + 0.5f * modulator);
        }

        return audio;
    }
}