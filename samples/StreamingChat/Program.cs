using Fluid.OpenVINO.GenAI;

namespace StreamingChat.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("OpenVINO.NET GenAI - Streaming Chat Sample");
        Console.WriteLine("==========================================");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: StreamingChat.Sample <model_path> [device]");
            Console.WriteLine("Example: StreamingChat.Sample C:\\models\\my-llm CPU");
            return;
        }

        string modelPath = args[0];
        string device = args.Length > 1 ? args[1] : "CPU";

        try
        {
            Console.WriteLine($"Loading model from: {modelPath}");
            Console.WriteLine($"Using device: {device}");
            Console.WriteLine();

            using var pipeline = new LLMPipeline(modelPath, device);

            // Create optimized configuration for chat
            var config = GenerationConfig.Default
                .WithMaxTokens(200)
                .WithTemperature(0.8f)
                .WithTopP(0.95f)
                .WithTopK(50)
                .WithSampling(true)
                .WithRepetitionPenalty(1.1f);

            Console.WriteLine("Chat Configuration:");
            Console.WriteLine($"  - Max tokens: {config.GetMaxNewTokens()}");
            Console.WriteLine($"  - Temperature: 0.8");
            Console.WriteLine($"  - Top-P: 0.95");
            Console.WriteLine($"  - Top-K: 50");
            Console.WriteLine();

            // Start an interactive chat session
            Console.WriteLine("Starting chat session... (type 'help' for commands, 'quit' to exit)");
            Console.WriteLine();

            using var chatSession = pipeline.StartChatSession();
            var cts = new CancellationTokenSource();

            // Handle Ctrl+C to cancel streaming
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("\n[Generation cancelled]");
            };

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput))
                    continue;

                switch (userInput.ToLower())
                {
                    case "quit":
                    case "exit":
                        return;

                    case "help":
                        ShowHelp();
                        continue;

                    case "clear":
                        Console.Clear();
                        continue;

                    case "config":
                        ShowConfig(config);
                        continue;
                }

                // Cancel any previous operation
                if (cts.Token.IsCancellationRequested)
                {
                    cts = new CancellationTokenSource();
                }

                Console.Write("Assistant: ");

                try
                {
                    var responseStartTime = DateTime.UtcNow;
                    var tokenCount = 0;

                    await foreach (var token in chatSession.SendMessageStreamAsync(userInput, config, cts.Token))
                    {
                        Console.Write(token);
                        tokenCount++;
                    }

                    var responseTime = DateTime.UtcNow - responseStartTime;
                    var tokensPerSecond = tokenCount / responseTime.TotalSeconds;

                    Console.WriteLine();
                    Console.WriteLine($"[{tokenCount} tokens, {tokensPerSecond:F1} tok/s]");
                    Console.WriteLine();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("\n[Response cancelled by user]");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Error: {ex.Message}]");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }

            Environment.Exit(1);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  help   - Show this help message");
        Console.WriteLine("  clear  - Clear the console");
        Console.WriteLine("  config - Show current generation configuration");
        Console.WriteLine("  quit   - Exit the chat");
        Console.WriteLine("  Ctrl+C - Cancel current generation");
        Console.WriteLine();
    }

    static void ShowConfig(GenerationConfig config)
    {
        Console.WriteLine("Current Configuration:");
        Console.WriteLine($"  - Max new tokens: {config.GetMaxNewTokens()}");
        Console.WriteLine("  - Temperature: 0.8");
        Console.WriteLine("  - Top-P: 0.95");
        Console.WriteLine("  - Top-K: 50");
        Console.WriteLine("  - Sampling: enabled");
        Console.WriteLine("  - Repetition penalty: 1.1");
        Console.WriteLine();
    }
}
