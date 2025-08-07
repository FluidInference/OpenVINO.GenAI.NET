using Fluid.OpenVINO.GenAI;

namespace SimpleStreamingCheck;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET GenAI - Simple Streaming Check");
        Console.WriteLine("===========================================");
        
        // Use environment variable or default model path
        string modelPath = Environment.GetEnvironmentVariable("QUICKDEMO_MODEL_PATH") 
            ?? @"C:\Users\brand\code\OpenVINO.GenAI.NET\Models\qwen3-0.6b-int4-ov-npu";
        
        string device = args.Length > 0 ? args[0] : "CPU";

        try
        {
            Console.WriteLine($"Model: {modelPath}");
            Console.WriteLine($"Device: {device}");
            Console.WriteLine();

            using var pipeline = new LLMPipeline(modelPath, device);
            
            var config = GenerationConfig.Default
                .WithMaxTokens(100)
                .WithTemperature(0.7f);

            // Test streaming with a simple prompt
            var prompt = "Count to 5:";
            Console.WriteLine($"Prompt: {prompt}");
            Console.Write("Streaming response: ");
            
            int tokenCount = 0;
            await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
            {
                Console.Write(token);
                tokenCount++;
                
                // Add a small delay to visualize streaming
                await Task.Delay(50);
            }
            
            Console.WriteLine();
            Console.WriteLine($"\n✓ Streaming works! Received {tokenCount} tokens");
            
            // Also test non-streaming for comparison
            Console.WriteLine("\nNon-streaming response: ");
            var result = await pipeline.GenerateAsync(prompt, config);
            Console.WriteLine(result.Text);
            
            Console.WriteLine("\n✓ Both streaming and non-streaming work!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}