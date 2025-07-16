# Contributing Guide

## Development Setup

### Prerequisites
- Visual Studio 2022 or VS Code with C# extension
- .NET 9.0 SDK
- OpenVINO GenAI runtime

### Getting Started

1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/OpenVINO.GenAI.NET.git
   cd OpenVINO.GenAI.NET
   ```

2. **Build and Test**
   ```bash
   dotnet build OpenVINO.NET.sln
   dotnet test tests/OpenVINO.NET.GenAI.Tests/
   ```

3. **Run Samples**
   ```bash
   dotnet run --project samples/QuickDemo
   ```

## Code Style Guidelines

### C# Coding Conventions
- Follow Microsoft C# coding conventions
- Use PascalCase for public members
- Use camelCase for private fields
- Use async/await patterns consistently
- Implement proper resource disposal (using statements)

### Documentation
- Add XML documentation for all public APIs
- Include usage examples in documentation
- Update README.md when adding new features
- Document breaking changes

### Example Code Style
```csharp
/// <summary>
/// Generates text using the specified prompt and configuration.
/// </summary>
/// <param name="prompt">The input prompt for text generation.</param>
/// <param name="config">Generation configuration parameters.</param>
/// <returns>Generated text response.</returns>
public async Task<string> GenerateAsync(string prompt, GenerationConfig config)
{
    using var handle = CreateHandle();
    return await ProcessAsync(handle, prompt, config);
}
```

## Adding New Features

### 1. P/Invoke Layer
Add native method declarations in `GenAINativeMethods.cs`:

```csharp
[DllImport("openvino_genai_c", CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr ov_genai_new_feature_create(
    [MarshalAs(UnmanagedType.LPStr)] string config);
```

### 2. SafeHandle Implementation
Create appropriate handle classes for resource management:

```csharp
public class NewFeatureSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public NewFeatureSafeHandle() : base(true) { }

    protected override bool ReleaseHandle()
    {
        GenAINativeMethods.ov_genai_new_feature_free(handle);
        return true;
    }
}
```

### 3. High-level API
Implement user-friendly wrapper classes:

```csharp
public class NewFeature : IDisposable
{
    private readonly NewFeatureSafeHandle _handle;

    public NewFeature(string config)
    {
        _handle = GenAINativeMethods.ov_genai_new_feature_create(config);
    }

    public void Dispose() => _handle?.Dispose();
}
```

### 4. Tests
Add comprehensive unit tests:

```csharp
[Test]
public async Task NewFeature_Should_Work_Correctly()
{
    // Arrange
    using var feature = new NewFeature("test-config");

    // Act
    var result = await feature.ProcessAsync("test-input");

    // Assert
    Assert.IsNotNull(result);
}
```

## Testing

### Unit Tests
- Write tests for all public APIs
- Use descriptive test names
- Test both success and failure scenarios
- Mock external dependencies

### Integration Tests
- Test with real OpenVINO runtime
- Test different device types
- Test with various model formats
- Performance regression tests

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/OpenVINO.NET.GenAI.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Pull Request Process

### Before Submitting
1. Ensure all tests pass
2. Update documentation
3. Follow code style guidelines
4. Add appropriate unit tests
5. Test on target platforms

### PR Description
Include:
- Clear description of changes
- Motivation for the change
- Testing performed
- Breaking changes (if any)
- Related issues

### Review Process
- Code reviews are required
- Address feedback promptly
- Ensure CI passes
- Maintain backwards compatibility when possible

## Project Structure

```
OpenVINO.GenAI.NET/
├── src/
│   ├── OpenVINO.NET.Core/          # Core OpenVINO functionality
│   ├── OpenVINO.NET.GenAI/         # GenAI high-level API
│   └── OpenVINO.NET.Native/        # Native interop layer
├── tests/
│   └── OpenVINO.NET.GenAI.Tests/   # Unit and integration tests
├── samples/                        # Sample applications
└── doc/                           # Documentation
```

## Best Practices

### Memory Management
- Always use SafeHandle for native resources
- Implement IDisposable correctly
- Use using statements for automatic cleanup

### Async Programming
- Use async/await throughout
- Support cancellation tokens
- Handle exceptions properly in async methods

### Error Handling
- Use specific exception types
- Provide meaningful error messages
- Include context in exceptions

### Performance
- Minimize allocations in hot paths
- Use streaming when appropriate
- Profile performance-critical code

## Getting Help

### Development Questions
- Check existing documentation
- Look at similar implementations
- Ask in GitHub discussions
- Join our Discord community

### Reporting Issues
- Use GitHub Issues for bugs
- Provide minimal reproduction cases
- Include environment information
- Follow the issue template

## Release Process

### Version Numbering
- Follow semantic versioning (SemVer)
- Major: Breaking changes
- Minor: New features, backwards compatible
- Patch: Bug fixes

### Release Checklist
- [ ] All tests pass
- [ ] Documentation updated
- [ ] Version numbers updated
- [ ] Release notes prepared
- [ ] Tagged in git
