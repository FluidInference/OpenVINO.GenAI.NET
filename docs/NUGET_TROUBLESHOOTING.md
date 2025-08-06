# NuGet Package Troubleshooting Guide

## Overview

The `Fluid.OpenVINO.GenAI` NuGet package includes both managed assemblies and native OpenVINO libraries. This guide helps troubleshoot common issues with the package.

## Package Structure

When installed, the package contains:
- **Managed Assembly**: `lib/net8.0/OpenVINO.NET.GenAI.dll`
- **Native Libraries**: `runtimes/[win-x64|linux-x64]/native/*.dll` or `*.so`
- **MSBuild Targets**: `build/Fluid.OpenVINO.GenAI.targets`

## Common Issues and Solutions

### Issue 1: Native Libraries Not Found

**Symptoms:**
- Build warnings about missing OpenVINO GenAI native libraries
- Runtime errors when trying to use LLMPipeline or WhisperPipeline
- `DllNotFoundException` or similar errors

**Solutions:**

1. **Verify Package Installation**
   ```bash
   dotnet list package
   ```
   Ensure `Fluid.OpenVINO.GenAI` is listed with the correct version.

2. **Check NuGet Cache**
   The package should be in:
   - Windows: `%USERPROFILE%\.nuget\packages\fluid.openvino.genai\[version]\`
   - Linux: `~/.nuget/packages/fluid.openvino.genai/[version]/`

3. **Verify Native Libraries Exist**
   Check for native libraries in the package:
   ```powershell
   # Windows
   dir "%USERPROFILE%\.nuget\packages\fluid.openvino.genai\2025.3.0-dev20250801\runtimes\win-x64\native"
   
   # Linux
   ls ~/.nuget/packages/fluid.openvino.genai/2025.3.0-dev20250801/runtimes/linux-x64/native
   ```

4. **Force Package Reinstallation**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore --force
   ```

### Issue 2: Libraries Not Copied to Output

**Symptoms:**
- Build succeeds but native DLLs are not in the output directory
- Application fails at runtime with missing DLL errors

**Solutions:**

1. **Check MSBuild Targets Execution**
   Build with diagnostic verbosity:
   ```bash
   dotnet build -v d | Select-String "OpenVINO"
   ```

2. **Set Environment Variable**
   If automatic discovery fails, set the `OPENVINO_RUNTIME_PATH`:
   ```powershell
   # Windows
   $env:OPENVINO_RUNTIME_PATH = "C:\path\to\openvino\runtime"
   
   # Linux
   export OPENVINO_RUNTIME_PATH="/path/to/openvino/runtime"
   ```

3. **Manual Copy as Workaround**
   Add to your `.csproj`:
   ```xml
   <Target Name="CopyOpenVINOLibraries" AfterTargets="Build">
     <Copy SourceFiles="$(NuGetPackageRoot)fluid.openvino.genai\2025.3.0-dev20250801\runtimes\win-x64\native\*.dll"
           DestinationFolder="$(OutputPath)"
           SkipUnchangedFiles="true" />
   </Target>
   ```

### Issue 3: Platform Mismatch

**Symptoms:**
- `BadImageFormatException` errors
- "Wrong format" or architecture mismatch errors

**Solutions:**

1. **Ensure x64 Platform**
   Your project must target x64:
   ```xml
   <PropertyGroup>
     <PlatformTarget>x64</PlatformTarget>
     <Platforms>x64</Platforms>
   </PropertyGroup>
   ```

2. **Verify Runtime Identifier**
   For self-contained deployments:
   ```xml
   <RuntimeIdentifier>win-x64</RuntimeIdentifier>
   <!-- or -->
   <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
   ```

### Issue 4: Linux-Specific Issues

**Symptoms:**
- Libraries found but fail to load on Linux
- Missing dependency errors

**Solutions:**

1. **Set LD_LIBRARY_PATH**
   ```bash
   export LD_LIBRARY_PATH=$PWD:$LD_LIBRARY_PATH
   dotnet run
   ```

2. **Check Dependencies**
   ```bash
   ldd libopenvino_genai_c.so
   ```

3. **Install System Dependencies**
   ```bash
   # Ubuntu/Debian
   sudo apt-get install libtbb2
   
   # RHEL/CentOS
   sudo yum install tbb
   ```

## Validation Script

Create a simple test project to validate the package:

```csharp
using System;
using System.IO;
using Fluid.OpenVINO.GenAI;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing Fluid.OpenVINO.GenAI...");
        
        // Test managed assembly
        var config = GenerationConfig.Default;
        Console.WriteLine($"✓ GenerationConfig created: MaxTokens={config.MaxTokens}");
        
        // Check native libraries
        var dlls = new[] { "openvino_genai_c.dll", "openvino_c.dll" };
        foreach (var dll in dlls)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dll);
            Console.WriteLine($"{dll}: {(File.Exists(path) ? "✓" : "✗")}");
        }
    }
}
```

## Package Contents Verification

To verify what's included in the NuGet package:

```powershell
# Extract and inspect package
Copy-Item "package.nupkg" -Destination "package.zip"
Expand-Archive -Path "package.zip" -DestinationPath "package_contents"
Get-ChildItem -Path "package_contents" -Recurse
```

## Build Output

When everything is working correctly, you should see:
- No warnings during build
- Native libraries copied to output directory
- Message: "OpenVINO GenAI: Copied native libraries from ... to ..."

## Getting Help

If issues persist:
1. Check the [GitHub Issues](https://github.com/FluidInference/OpenVINO.GenAI.NET/issues)
2. Enable MSBuild diagnostic logging: `dotnet build -v diag > build.log`
3. Report issues with:
   - .NET version (`dotnet --version`)
   - Operating system and architecture
   - Package version
   - Build log excerpt

## Environment Variables

- `OPENVINO_RUNTIME_PATH`: Override automatic library discovery
- `CI`: Set to `true` to suppress warnings in CI/CD environments
- `SuppressOpenVINOWarnings`: Set to `true` to disable all warnings