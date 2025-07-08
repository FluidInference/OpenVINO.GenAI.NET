# OpenVINO.NET Scripts

This directory contains utility scripts for the OpenVINO.NET project.

## Scripts

### download-openvino-runtime.ps1

Downloads and sets up the OpenVINO GenAI runtime for Windows x64. This script is used by CI/CD pipelines and can also be run locally for development.

**Usage:**
```powershell
# Download default version (2025.2.0.0rc4)
.\download-openvino-runtime.ps1

# Download specific version
.\download-openvino-runtime.ps1 -Version "2025.2.0.0rc4" -OutputPath "custom/path"
```

**What it does:**
1. Downloads the OpenVINO GenAI runtime ZIP from the official repository
2. Extracts it to the build/native directory
3. Copies DLLs to the expected runtime location
4. Verifies the installation

**Requirements:**
- Windows PowerShell 5.0 or later
- Internet connection
- ~500MB disk space

## CI/CD Integration

These scripts are automatically used by GitHub Actions workflows:
- PR validation workflow runs basic tests without downloading runtime
- Main CI workflow downloads runtime and runs full integration tests