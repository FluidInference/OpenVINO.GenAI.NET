using System.CommandLine;
using System.Diagnostics;
using OpenVINO.NET.GenAI;

namespace QuickDemo;

class Program
{
    // Fixed configuration for simplicity
    private const string ModelId = "OpenVINO/Qwen3-0.6B-fp16-ov";
    private const string ModelsDirectory = "./Models";
    private const float Temperature = 0.7f;
    private const int MaxTokens = 100;
    private const float TopP = 0.9f;

    // Predefined prompts for consistent testing
    private static readonly string[] TestPrompts = new[]
    {
        "Explain quantum computing in simple terms:",
        "Write a short poem about artificial intelligence:",
        "What are the benefits of renewable energy?",
        "Describe the process of making coffee:"
    };

    static async Task<int> Main(string[] args)
    {
        var deviceOption = new Option<string>(
            name: "--device",
            description: "Device to run inference on (CPU, GPU, NPU)",
            getDefaultValue: () => "CPU");

        var benchmarkOption = new Option<bool>(
            name: "--benchmark",
            description: "Run benchmark comparing all available devices");

        var rootCommand = new RootCommand("OpenVINO.NET Quick Demo - Showcase LLM inference capabilities")
        {
            deviceOption,
            benchmarkOption
        };

        rootCommand.SetHandler(async (device, benchmark) =>
        {
            await RunDemoAsync(device, benchmark);
        }, deviceOption, benchmarkOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunDemoAsync(string device, bool benchmark)
    {
        try
        {
            Console.WriteLine("OpenVINO.NET Quick Demo");
            Console.WriteLine("=======================");
            Console.WriteLine($"Model: {ModelId.Split('/')[1]}");
            Console.WriteLine($"Temperature: {Temperature}, Max Tokens: {MaxTokens}");
            Console.WriteLine();

            // Ensure model is downloaded
            var modelPath = await EnsureModelDownloadedAsync();

            if (benchmark)
            {
                await RunBenchmarkAsync(modelPath);
            }
            else
            {
                await RunSingleDeviceDemoAsync(modelPath, device);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure you have the required OpenVINO runtime installed.");
            Environment.Exit(1);
        }
    }

    static async Task<string> EnsureModelDownloadedAsync()
    {
        // Check for environment variable override (used in CI)
        var envModelPath = Environment.GetEnvironmentVariable("QUICKDEMO_MODEL_PATH");
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
        Console.WriteLine("This may take a few minutes for the first run (~1.2GB)...");
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

        // For simplicity, we'll download the key files needed for OpenVINO GenAI
        var files = new[]
        {
            "openvino_model.xml",
            "openvino_model.bin",
            "openvino_tokenizer.xml",
            "openvino_tokenizer.bin",
            "openvino_detokenizer.xml",
            "openvino_detokenizer.bin",
            "config.json",
            "generation_config.json"
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
            using var pipeline = new LLMPipeline(modelPath, device.ToUpper());
            var config = GenerationConfig.Default
                .WithMaxTokens(MaxTokens)
                .WithTemperature(Temperature)
                .WithTopP(TopP)
                .WithSampling(true);

            for (int i = 0; i < TestPrompts.Length; i++)
            {
                Console.WriteLine($"Prompt {i + 1}: \"{TestPrompts[i]}\"");

                var stopwatch = Stopwatch.StartNew();
                var firstTokenTime = TimeSpan.Zero;
                var tokenCount = 0;
                var response = "";

                Console.Write("Response: \"");

                await foreach (var token in pipeline.GenerateStreamAsync(TestPrompts[i], config))
                {
                    if (tokenCount == 0)
                    {
                        firstTokenTime = stopwatch.Elapsed;
                    }

                    Console.Write(token);
                    response += token;
                    tokenCount++;
                }

                stopwatch.Stop();
                Console.WriteLine("\"");

                var totalTime = stopwatch.Elapsed;
                var tokensPerSecond = tokenCount / totalTime.TotalSeconds;

                Console.WriteLine($"Performance: {tokensPerSecond:F1} tokens/sec, First token: {firstTokenTime.TotalMilliseconds:F0}ms");
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
                using var pipeline = new LLMPipeline(modelPath, device);
                var config = GenerationConfig.Default
                    .WithMaxTokens(MaxTokens)
                    .WithTemperature(Temperature)
                    .WithTopP(TopP)
                    .WithSampling(true);

                var deviceResults = new List<double>();
                var firstTokenLatencies = new List<double>();

                // Run a subset of prompts for benchmarking
                var benchmarkPrompts = TestPrompts.Take(2).ToArray();

                foreach (var prompt in benchmarkPrompts)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var firstTokenTime = TimeSpan.Zero;
                    var tokenCount = 0;

                    await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
                    {
                        if (tokenCount == 0)
                        {
                            firstTokenTime = stopwatch.Elapsed;
                        }
                        tokenCount++;
                    }

                    stopwatch.Stop();
                    var tokensPerSecond = tokenCount / stopwatch.Elapsed.TotalSeconds;

                    deviceResults.Add(tokensPerSecond);
                    firstTokenLatencies.Add(firstTokenTime.TotalMilliseconds);
                }

                var avgTokensPerSecond = deviceResults.Average();
                var avgFirstTokenMs = firstTokenLatencies.Average();

                results.Add(new BenchmarkResult
                {
                    Device = device,
                    TokensPerSecond = avgTokensPerSecond,
                    FirstTokenLatencyMs = avgFirstTokenMs,
                    Success = true
                });

                Console.WriteLine($"✓ {device}: {avgTokensPerSecond:F1} tokens/sec");
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
        Console.WriteLine("Device    | Tokens/sec | First Token | Status");
        Console.WriteLine("--------- | ---------- | ----------- | ------");

        foreach (var result in results)
        {
            if (result.Success)
            {
                Console.WriteLine($"{result.Device,-9} | {result.TokensPerSecond,10:F1} | {result.FirstTokenLatencyMs,8:F0}ms | ✓");
            }
            else
            {
                Console.WriteLine($"{result.Device,-9} | {"N/A",10} | {"N/A",11} | ✗");
            }
        }

        Console.WriteLine();

        // Show winner
        var bestResult = results.Where(r => r.Success).OrderByDescending(r => r.TokensPerSecond).FirstOrDefault();
        if (bestResult != null)
        {
            Console.WriteLine($"🏆 Best Performance: {bestResult.Device} ({bestResult.TokensPerSecond:F1} tokens/sec)");
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

    class BenchmarkResult
    {
        public string Device { get; set; } = "";
        public double TokensPerSecond { get; set; }
        public double FirstTokenLatencyMs { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}