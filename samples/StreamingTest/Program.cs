using Fluid.OpenVINO.GenAI;
using System.Diagnostics;

namespace StreamingTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET GenAI - Streaming Test");
        Console.WriteLine("===================================");
        Console.WriteLine("This sample verifies that token streaming works correctly.");
        Console.WriteLine();

        // Use environment variable or command line argument for model path
        string modelPath = Environment.GetEnvironmentVariable("QUICKDEMO_MODEL_PATH") 
            ?? (args.Length > 0 ? args[0] : @"C:\Users\brand\code\OpenVINO.GenAI.NET\Models\qwen3-0.6b-int4-ov-npu");
        
        string device = args.Length > 1 ? args[1] : "CPU";

        try
        {
            Console.WriteLine($"Loading model: {modelPath}");
            Console.WriteLine($"Device: {device}");
            Console.WriteLine();

            using var pipeline = new LLMPipeline(modelPath, device);
            
            var config = GenerationConfig.Default
                .WithMaxTokens(50)
                .WithTemperature(0.7f)
                .WithSampling(true);

            // Test 1: Basic streaming
            await TestBasicStreaming(pipeline, config);
            
            // Test 2: Verify tokens arrive incrementally
            await TestIncrementalStreaming(pipeline, config);
            
            // Test 3: Test cancellation
            await TestCancellation(pipeline, config);
            
            // Test 4: Compare streaming vs non-streaming
            await CompareStreamingMethods(pipeline, config);

            Console.WriteLine();
            Console.WriteLine("✓ All streaming tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }

    static async Task TestBasicStreaming(LLMPipeline pipeline, GenerationConfig config)
    {
        Console.WriteLine("Test 1: Basic Streaming");
        Console.WriteLine("-----------------------");
        
        var prompt = "Count from 1 to 5:";
        Console.WriteLine($"Prompt: {prompt}");
        Console.Write("Response: ");
        
        int tokenCount = 0;
        await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
        {
            Console.Write(token);
            tokenCount++;
        }
        
        Console.WriteLine();
        Console.WriteLine($"✓ Received {tokenCount} tokens");
        Console.WriteLine();
    }

    static async Task TestIncrementalStreaming(LLMPipeline pipeline, GenerationConfig config)
    {
        Console.WriteLine("Test 2: Incremental Token Arrival");
        Console.WriteLine("----------------------------------");
        
        var prompt = "Hello, world";
        Console.WriteLine($"Prompt: {prompt}");
        
        var timestamps = new List<long>();
        var tokens = new List<string>();
        var sw = Stopwatch.StartNew();
        
        await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
        {
            timestamps.Add(sw.ElapsedMilliseconds);
            tokens.Add(token);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Received {tokens.Count} tokens over {sw.ElapsedMilliseconds}ms");
        
        // Verify tokens arrived incrementally (not all at once)
        if (timestamps.Count > 1)
        {
            var gaps = new List<long>();
            for (int i = 1; i < timestamps.Count; i++)
            {
                gaps.Add(timestamps[i] - timestamps[i - 1]);
            }
            
            var avgGap = gaps.Average();
            var maxGap = gaps.Max();
            
            Console.WriteLine($"Average time between tokens: {avgGap:F1}ms");
            Console.WriteLine($"Max time between tokens: {maxGap}ms");
            
            // If all tokens arrived at once, the first gap would be huge and others would be ~0
            if (maxGap > avgGap * 10 && gaps.Count(g => g < 5) > gaps.Count / 2)
            {
                Console.WriteLine("⚠ Warning: Tokens may not be streaming properly (arriving in batches)");
            }
            else
            {
                Console.WriteLine("✓ Tokens arrived incrementally");
            }
        }
        
        Console.WriteLine();
    }

    static async Task TestCancellation(LLMPipeline pipeline, GenerationConfig config)
    {
        Console.WriteLine("Test 3: Cancellation");
        Console.WriteLine("--------------------");
        
        var prompt = "Write a very long story about artificial intelligence:";
        Console.WriteLine($"Prompt: {prompt}");
        
        using var cts = new CancellationTokenSource();
        
        int tokenCount = 0;
        try
        {
            await foreach (var token in pipeline.GenerateStreamAsync(prompt, config, cts.Token))
            {
                tokenCount++;
                // Cancel after 5 tokens
                if (tokenCount >= 5)
                {
                    cts.Cancel();
                }
            }
            
            Console.WriteLine("✗ Cancellation did not work as expected");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"✓ Successfully cancelled after {tokenCount} tokens");
        }
        
        Console.WriteLine();
    }

    static async Task CompareStreamingMethods(LLMPipeline pipeline, GenerationConfig config)
    {
        Console.WriteLine("Test 4: Compare Streaming vs Non-Streaming");
        Console.WriteLine("-------------------------------------------");
        
        var prompt = "What is 2+2?";
        var shortConfig = config.WithMaxTokens(20);
        
        Console.WriteLine($"Prompt: {prompt}");
        
        // Non-streaming
        Console.Write("Non-streaming: ");
        var sw1 = Stopwatch.StartNew();
        var nonStreamResult = await pipeline.GenerateAsync(prompt, shortConfig);
        sw1.Stop();
        Console.WriteLine($"{nonStreamResult.Text.Length} chars in {sw1.ElapsedMilliseconds}ms");
        
        // Streaming
        Console.Write("Streaming: ");
        var sw2 = Stopwatch.StartNew();
        var streamResult = "";
        long firstTokenTime = 0;
        bool firstToken = true;
        
        await foreach (var token in pipeline.GenerateStreamAsync(prompt, shortConfig))
        {
            if (firstToken)
            {
                firstTokenTime = sw2.ElapsedMilliseconds;
                firstToken = false;
            }
            streamResult += token;
        }
        sw2.Stop();
        
        Console.WriteLine($"{streamResult.Length} chars in {sw2.ElapsedMilliseconds}ms (first token at {firstTokenTime}ms)");
        
        // Compare results
        if (Math.Abs(nonStreamResult.Text.Length - streamResult.Length) <= 5) // Allow small difference
        {
            Console.WriteLine("✓ Both methods produced similar output length");
        }
        else
        {
            Console.WriteLine($"⚠ Output lengths differ significantly: {nonStreamResult.Text.Length} vs {streamResult.Length}");
        }
        
        if (firstTokenTime < sw1.ElapsedMilliseconds)
        {
            Console.WriteLine($"✓ Streaming delivered first token faster ({firstTokenTime}ms vs {sw1.ElapsedMilliseconds}ms total)");
        }
        
        Console.WriteLine();
    }
}