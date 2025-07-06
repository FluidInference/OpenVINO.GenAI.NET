using OpenVINO.NET.GenAI;

namespace TextGeneration.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET GenAI - Text Generation Sample");
        Console.WriteLine("==========================================");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TextGeneration.Sample <model_path> [device]");
            Console.WriteLine("Example: TextGeneration.Sample C:\\models\\my-llm CPU");
            return;
        }

        string modelPath = args[0];
        string device = args.Length > 1 ? args[1] : "CPU";

        try
        {
            // Initialize the LLM pipeline
            Console.WriteLine($"Loading model from: {modelPath}");
            Console.WriteLine($"Using device: {device}");
            Console.WriteLine();

            using var pipeline = new LLMPipeline(modelPath, device);

            // Create generation configuration
            var config = GenerationConfig.Default
                .WithMaxTokens(100)
                .WithTemperature(0.7f)
                .WithTopP(0.9f)
                .WithSampling(true);

            // Example 1: Simple text generation
            Console.WriteLine("=== Simple Text Generation ===");
            string prompt = "The future of artificial intelligence is";
            Console.WriteLine($"Prompt: {prompt}");
            Console.WriteLine("Response:");

            using var result = pipeline.Generate(prompt, config);
            Console.WriteLine(result.Text);
            Console.WriteLine();

            // Display performance metrics
            var metrics = result.PerformanceMetrics;
            Console.WriteLine("Performance Metrics:");
            Console.WriteLine($"  - Tokens per second: {metrics.TokensPerSecond:F2}");
            Console.WriteLine($"  - First token latency: {metrics.FirstTokenLatency:F2} ms");
            Console.WriteLine($"  - Input tokens: {metrics.NumInputTokens}");
            Console.WriteLine($"  - Generated tokens: {metrics.NumGenerationTokens}");
            Console.WriteLine();

            // Example 2: Streaming generation
            Console.WriteLine("=== Streaming Text Generation ===");
            prompt = "Once upon a time in a magical forest";
            Console.WriteLine($"Prompt: {prompt}");
            Console.WriteLine("Streaming response:");

            await foreach (var token in pipeline.GenerateStreamAsync(prompt, config))
            {
                Console.Write(token);
            }
            Console.WriteLine();
            Console.WriteLine();

            // Example 3: Interactive chat
            Console.WriteLine("=== Interactive Chat (type 'quit' to exit) ===");
            using var chatSession = pipeline.StartChatSession();

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "quit")
                    break;

                Console.Write("Assistant: ");

                // Stream the response for better user experience
                await foreach (var token in chatSession.SendMessageStreamAsync(userInput, config))
                {
                    Console.Write(token);
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            
            Environment.Exit(1);
        }
    }
}