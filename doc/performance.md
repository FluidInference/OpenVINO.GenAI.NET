# Performance Benchmarks

## Expected Performance (Qwen3-0.6B-fp16-ov)

| Device | Tokens/Second | First Token Latency | Notes |
|--------|---------------|-------------------|--------|
| CPU    | 12-15         | 400-600ms        | Always available |
| GPU    | 20-30         | 200-400ms        | Requires compatible GPU |
| NPU    | 15-25         | 300-500ms        | Intel NPU required |

## Benchmark Command

```bash
# Compare all available devices
dotnet run --project samples/QuickDemo -- --benchmark
```

## Performance Factors

### Hardware Requirements
- **CPU**: Intel Core processors (recommended i5 or better)
- **GPU**: Intel Arc or compatible GPU with OpenVINO support
- **NPU**: Intel Neural Processing Unit (available on newer Intel processors)
- **Memory**: 8GB+ RAM recommended for optimal performance

### Model Size Impact
- **Larger models**: Better quality, slower inference
- **Smaller models**: Faster inference, potentially lower quality
- **Quantized models**: Faster inference with minimal quality loss

### Optimization Tips

1. **Device Selection**
   - Use GPU for best performance if available
   - NPU provides good balance of performance and power efficiency
   - CPU is always available as fallback

2. **Model Optimization**
   - Use quantized models (INT8/INT4) for faster inference
   - Consider model size vs. quality trade-offs
   - Pre-compile models for target devices

3. **Configuration Tuning**
   - Adjust `max_tokens` based on your needs
   - Use appropriate `temperature` settings
   - Consider batch processing for multiple requests

## Measuring Performance

### Built-in Metrics
The library provides performance metrics through the `PerformanceMetrics` class:

```csharp
var result = await pipeline.GenerateAsync(prompt, config);
var metrics = result.PerfMetrics;

Console.WriteLine($"Tokens/sec: {metrics.TokensPerSecond:F2}");
Console.WriteLine($"First token latency: {metrics.FirstTokenLatency:F0}ms");
```

### Custom Benchmarking
For detailed performance analysis, use the benchmark mode:

```bash
dotnet run --project samples/QuickDemo -- --benchmark --iterations 10
```

## Performance Troubleshooting

### Slow Performance
- Check device availability and selection
- Verify model is properly optimized
- Monitor system resources (CPU, memory usage)
- Consider using smaller/quantized models

### High Latency
- First token latency is typically higher than subsequent tokens
- Model loading time affects initial latency
- Consider keeping pipeline instances alive for better performance

### Memory Issues
- Monitor memory usage during inference
- Use streaming generation for long texts
- Consider model size vs. available memory
