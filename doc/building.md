# Building Guide

## Prerequisites

- .NET 9.0 SDK or later
- OpenVINO GenAI runtime 2025.2.0.0-rc4
- Visual Studio 2022 or VS Code (optional, for development)

## Build Commands

### Standard Build
```bash
dotnet build OpenVINO.NET.sln
```

### Clean Build
```bash
dotnet clean OpenVINO.NET.sln
dotnet build OpenVINO.NET.sln
```

### Release Build
```bash
dotnet build OpenVINO.NET.sln --configuration Release
```

### Build Individual Projects
```bash
# Build core library
dotnet build src/OpenVINO.NET.Core/

# Build GenAI library
dotnet build src/OpenVINO.NET.GenAI/

# Build native interop
dotnet build src/OpenVINO.NET.Native/
```

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/OpenVINO.NET.GenAI.Tests/
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Packaging

### Create NuGet Packages
```bash
dotnet pack OpenVINO.NET.sln --configuration Release
```

### Pack Individual Projects
```bash
dotnet pack src/OpenVINO.NET.GenAI/ --configuration Release
```

## Build Configuration

### Debug (Default)
- Includes debug symbols
- No optimizations
- Detailed error messages

### Release
- Optimized for performance
- Minimal debug information
- Suitable for production

## Platform Support

Currently supports:
- Windows x64
- .NET 9.0 or later

## Build Troubleshooting

### Common Issues

1. **Missing .NET SDK**
   ```bash
   dotnet --version  # Check if .NET is installed
   ```

2. **Missing OpenVINO Runtime**
   - Ensure OpenVINO GenAI runtime is installed
   - Check PATH environment variable

3. **Build Errors**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

For more detailed troubleshooting, see the [Troubleshooting Guide](troubleshooting.md).
