# Installation Guide

## Requirements

- .NET 9.0 or later
- Windows 11
- Windows x64
- OpenVINO GenAI 2025.2.0.0-rc4 runtime

## Prerequisites

### 1. Install .NET 9.0 SDK or later
Download from: https://dotnet.microsoft.com/download

### 2. Install OpenVINO GenAI Runtime 2025.2.0.0-rc4
- Download from: https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/
- Extract to a directory in your PATH, or place DLLs in your application's output directory

## Build from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/OpenVINO-C-Sharp.git
cd OpenVINO-C-Sharp

# Build the solution
dotnet build OpenVINO.NET.sln

# Run the quick demo
dotnet run --project samples/QuickDemo
```

## Environment Setup

### Option 1: System PATH (Recommended)
Add the OpenVINO GenAI runtime directory to your system PATH environment variable.

### Option 2: Application Directory
Copy the OpenVINO GenAI runtime DLLs to your application's output directory.

### Option 3: Using Scripts
Use the provided PowerShell script to download and set up the runtime:

```powershell
# Run from the project root
./scripts/download-openvino-runtime.ps1
```

## Verification

After installation, verify everything is working:

```bash
# Test the installation
dotnet run --project samples/QuickDemo

# Check device availability
dotnet run --project samples/QuickDemo -- --benchmark
```

## Troubleshooting Installation

### Common Issues

1. **Missing Runtime DLLs**
   - Ensure OpenVINO GenAI runtime is properly installed
   - Check that DLLs are in PATH or application directory

2. **Wrong .NET Version**
   - Verify you have .NET 9.0 or later installed
   - Check with: `dotnet --version`

3. **Build Errors**
   - Ensure all prerequisites are installed
   - Try cleaning and rebuilding: `dotnet clean && dotnet build`

For more detailed troubleshooting, see the [Troubleshooting Guide](troubleshooting.md).
