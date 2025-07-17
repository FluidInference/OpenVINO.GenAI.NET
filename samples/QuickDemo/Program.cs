using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using OpenVINO.NET.GenAI;

namespace QuickDemo;

class Program
{
    // Fixed configuration for simplicity
    private const string ModelId = "FluidInference/qwen3-0.6b-int4-ov-npu";
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

        var memoryMonitoringOption = new Option<bool>(
            name: "--memory-monitoring",
            description: "Enable detailed memory monitoring and reporting");

        var rootCommand = new RootCommand("OpenVINO.NET Quick Demo - Showcase LLM inference capabilities")
        {
            deviceOption,
            benchmarkOption,
            memoryMonitoringOption
        };

        rootCommand.SetHandler(async (device, benchmark, memoryMonitoring) =>
        {
            await RunDemoAsync(device, benchmark, memoryMonitoring);
        }, deviceOption, benchmarkOption, memoryMonitoringOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunDemoAsync(string device, bool benchmark, bool memoryMonitoring = false)
    {
        try
        {
            Console.WriteLine("OpenVINO.NET Quick Demo");
            Console.WriteLine("=======================");
            Console.WriteLine($"Model: {ModelId}");
            Console.WriteLine($"Temperature: {Temperature}, Max Tokens: {MaxTokens}");
            Console.WriteLine();

            // Ensure model is downloaded
            var modelPath = await EnsureModelDownloadedAsync();

            if (benchmark)
            {
                await RunBenchmarkAsync(modelPath, memoryMonitoring);
            }
            else
            {
                await RunSingleDeviceDemoAsync(modelPath, device, memoryMonitoring);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure you have the required OpenVINO runtime installed.");
            
            // Provide detailed diagnostics for DLL loading issues
            if (ex is DllNotFoundException || ex.Message.Contains("openvino_genai_c"))
            {
                Console.WriteLine();
                Console.WriteLine("=== OpenVINO GenAI Diagnostic Information ===");
                try
                {
                    var diagnosticInfo = OpenVINO.NET.GenAI.Native.NativeLibraryLoader.GetDiagnosticInfo();
                    Console.WriteLine(diagnosticInfo);
                }
                catch (Exception diagnosticEx)
                {
                    Console.WriteLine($"Failed to get diagnostic info: {diagnosticEx.Message}");
                }
                
                Console.WriteLine();
                Console.WriteLine("=== Current Directory Files ===");
                try
                {
                    var currentDir = Environment.CurrentDirectory;
                    Console.WriteLine($"Current directory: {currentDir}");
                    var dllFiles = Directory.GetFiles(currentDir, "*.dll", SearchOption.TopDirectoryOnly);
                    if (dllFiles.Any())
                    {
                        Console.WriteLine("DLL files found:");
                        foreach (var file in dllFiles.OrderBy(f => f))
                        {
                            var fileInfo = new FileInfo(file);
                            Console.WriteLine($"  {Path.GetFileName(file)} ({fileInfo.Length} bytes)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No DLL files found in current directory.");
                    }
                }
                catch (Exception dirEx)
                {
                    Console.WriteLine($"Failed to list directory contents: {dirEx.Message}");
                }
                
                Console.WriteLine();
                Console.WriteLine("=== Runtime Directory Files ===");
                try
                {
                    var runtimeDir = Path.Combine(Environment.CurrentDirectory, "runtimes", "win-x64", "native");
                    if (Directory.Exists(runtimeDir))
                    {
                        Console.WriteLine($"Runtime directory: {runtimeDir}");
                        var runtimeFiles = Directory.GetFiles(runtimeDir, "*.dll", SearchOption.TopDirectoryOnly);
                        if (runtimeFiles.Any())
                        {
                            Console.WriteLine("Runtime DLL files found:");
                            foreach (var file in runtimeFiles.OrderBy(f => f))
                            {
                                var fileInfo = new FileInfo(file);
                                Console.WriteLine($"  {Path.GetFileName(file)} ({fileInfo.Length} bytes)");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No DLL files found in runtime directory.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Runtime directory does not exist: {runtimeDir}");
                    }
                }
                catch (Exception runtimeEx)
                {
                    Console.WriteLine($"Failed to list runtime directory contents: {runtimeEx.Message}");
                }
            }
            
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

    static async Task RunSingleDeviceDemoAsync(string modelPath, string device, bool memoryMonitoring = false)
    {
        Console.WriteLine($"Device: {device.ToUpper()}");
        Console.WriteLine();

        PerformanceMetrics? overallMetrics = null;

        try
        {
            using var pipeline = new LLMPipeline(modelPath, device.ToUpper());
            var config = GenerationConfig.Default
                .WithMaxTokens(MaxTokens)
                .WithTemperature(Temperature)
                .WithTopP(TopP)
                .WithSampling(true);

            if (memoryMonitoring)
            {
                overallMetrics = new PerformanceMetrics { Device = device.ToUpper() };
            }

            for (int i = 0; i < TestPrompts.Length; i++)
            {
                Console.WriteLine($"Prompt {i + 1}: \"{TestPrompts[i]}\"");

                var initialMemory = memoryMonitoring ? GC.GetTotalMemory(false) : 0;
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
                var finalMemory = memoryMonitoring ? GC.GetTotalMemory(false) : 0;
                var memoryUsedMB = memoryMonitoring ? (finalMemory - initialMemory) / 1024.0 / 1024.0 : 0;

                Console.WriteLine($"Performance: {tokensPerSecond:F1} tokens/sec, First token: {firstTokenTime.TotalMilliseconds:F0}ms");

                if (memoryMonitoring)
                {
                    Console.WriteLine($"Memory: {memoryUsedMB:F1}MB used, {finalMemory / 1024.0 / 1024.0:F1}MB total");

                    overallMetrics?.Iterations.Add(new IterationMetrics
                    {
                        Iteration = i + 1,
                        TokensPerSecond = tokensPerSecond,
                        FirstTokenLatencyMs = firstTokenTime.TotalMilliseconds,
                        TotalTimeMs = totalTime.TotalMilliseconds,
                        MemoryUsedMB = memoryUsedMB,
                        TotalMemoryMB = finalMemory / 1024.0 / 1024.0,
                        TokenCount = tokenCount,
                        Prompt = TestPrompts[i],
                        Response = response
                    });
                }

                Console.WriteLine();
            }

            if (memoryMonitoring && overallMetrics != null)
            {
                await SavePerformanceMetricsAsync(overallMetrics);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run on {device}: {ex.Message}");
            if (device.ToUpper() != "CPU")
            {
                Console.WriteLine("Trying CPU fallback...");
                await RunSingleDeviceDemoAsync(modelPath, "CPU", memoryMonitoring);
            }
        }
    }

    static async Task RunBenchmarkAsync(string modelPath, bool memoryMonitoring = false)
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

    class PerformanceMetrics
    {
        public string Device { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<IterationMetrics> Iterations { get; set; } = new();
        public double AverageTokensPerSecond => Iterations.Count > 0 ? Iterations.Average(i => i.TokensPerSecond) : 0;
        public double AverageFirstTokenLatencyMs => Iterations.Count > 0 ? Iterations.Average(i => i.FirstTokenLatencyMs) : 0;
        public double MaxMemoryUsedMB => Iterations.Count > 0 ? Iterations.Max(i => i.MemoryUsedMB) : 0;
        public double MaxTotalMemoryMB => Iterations.Count > 0 ? Iterations.Max(i => i.TotalMemoryMB) : 0;
    }

    class IterationMetrics
    {
        public int Iteration { get; set; }
        public double TokensPerSecond { get; set; }
        public double FirstTokenLatencyMs { get; set; }
        public double TotalTimeMs { get; set; }
        public double MemoryUsedMB { get; set; }
        public double TotalMemoryMB { get; set; }
        public int TokenCount { get; set; }
        public string Prompt { get; set; } = "";
        public string Response { get; set; } = "";
    }

    static async Task SavePerformanceMetricsAsync(PerformanceMetrics metrics)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var filename = $"performance-metrics-{metrics.Device.ToLower()}-{timestamp}.json";

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(metrics, options);
            await File.WriteAllTextAsync(filename, json);

            Console.WriteLine($"Performance metrics saved to: {filename}");

            // Also write a summary for CI/workflow consumption
            var summaryFilename = $"performance-summary-{metrics.Device.ToLower()}-{timestamp}.txt";
            var summary = $"""
                Performance Summary - {metrics.Device}
                Generated: {metrics.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
                
                Average Metrics:
                - Tokens per second: {metrics.AverageTokensPerSecond:F2}
                - First token latency: {metrics.AverageFirstTokenLatencyMs:F0}ms
                - Max memory used: {metrics.MaxMemoryUsedMB:F1}MB
                - Max total memory: {metrics.MaxTotalMemoryMB:F1}MB
                
                Iterations: {metrics.Iterations.Count}
                """;

            await File.WriteAllTextAsync(summaryFilename, summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to save performance metrics: {ex.Message}");
        }
    }
}
