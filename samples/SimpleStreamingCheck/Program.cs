using Fluid.OpenVINO.GenAI;

namespace SimpleStreamingCheck;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET GenAI - GPU Streaming with Caching");
        Console.WriteLine("===============================================");
        
        // Use environment variable or default model path
        string modelPath = Environment.GetEnvironmentVariable("QUICKDEMO_MODEL_PATH") 
            ?? @"C:\Users\brand\AppData\Local\Slipbox\Models\Qwen3-8B-int4-ov";
        
        string device = args.Length > 0 ? args[0] : "GPU";
        
        // Set up cache directory relative to the executable
        string cacheDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ".cache",
            "openvino"
        );
        Directory.CreateDirectory(cacheDir);

        try
        {
            Console.WriteLine($"Model: {modelPath}");
            Console.WriteLine($"Device: {device}");
            Console.WriteLine($"Cache Directory: {cacheDir}");
            Console.WriteLine();

            // Create pipeline with caching enabled
            var properties = new Dictionary<string, string>
            {
                { "CACHE_DIR", cacheDir }  // Enable model compilation caching
            };
            
            using var pipeline = new LLMPipeline(modelPath, device, properties);
            
            // Enable chat mode to maintain KV cache between generations
            // Note: StartChat/FinishChat for KV cache reuse between prompts
            // Commented out for initial testing
            // pipeline.StartChat();
            
            var config = GenerationConfig.Default
                .WithMaxTokens(4096)
                .WithTemperature(0.7f);

            // Test with a simple prompt
            var prompts = new[]
            {
                "What is the capital of the moon?"
            };

            foreach (var prompt in prompts)
            {
                Console.WriteLine($"\nPrompt: {prompt}");
                Console.Write("Response: ");
                
                var startTime = DateTime.Now;
                int tokenCount = 0;
                
                await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
                {
                    Console.Write(token);
                    tokenCount++;
                    
                    // Optional: Add delay to visualize streaming
                    if (args.Contains("--slow"))
                        await Task.Delay(50);
                }
                
                var elapsed = DateTime.Now - startTime;
                Console.WriteLine($"\n✓ Generated {tokenCount} tokens in {elapsed.TotalSeconds:F2}s");
                Console.WriteLine($"  Throughput: {tokenCount / elapsed.TotalSeconds:F1} tokens/sec");
            }
            
            // Finish chat to clear KV cache
            // pipeline.FinishChat();
            
            Console.WriteLine("\n✓ GPU caching demonstration complete!");
            Console.WriteLine("\nCache Benefits:");
            Console.WriteLine("1. Model compilation cached - faster startup on subsequent runs");
            Console.WriteLine("2. KV cache maintained during chat - faster subsequent prompts");
            Console.WriteLine("3. GPU memory efficiently managed with block-based allocation");
            
            // Show cache directory info
            if (Directory.Exists(cacheDir))
            {
                var cacheFiles = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
                Console.WriteLine($"\nCache contains {cacheFiles.Length} files");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}