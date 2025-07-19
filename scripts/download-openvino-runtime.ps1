<#
.SYNOPSIS
    Downloads and sets up OpenVINO GenAI runtime for Windows x64
.DESCRIPTION
    This script downloads the OpenVINO GenAI runtime package and extracts it to the build directory.
    It's used by CI/CD pipelines and can also be run locally for development.
.PARAMETER Version
    The version of OpenVINO GenAI to download (default: 2025.2.0.0)
.PARAMETER OutputPath
    The path where the runtime should be extracted (default: build/native)
.EXAMPLE
    .\download-openvino-runtime.ps1
.EXAMPLE
    .\download-openvino-runtime.ps1 -Version "2025.2.0.0" -OutputPath "custom/path"
#>

param(
    [string]$Version = "2025.2.0.0",
    [string]$OutputPath = "build/native"
)

$ErrorActionPreference = "Stop"

Write-Host "OpenVINO GenAI Runtime Downloader" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Version: $Version"
Write-Host "Output Path: $OutputPath"
Write-Host ""

# Create output directory
$fullOutputPath = Join-Path $PSScriptRoot ".."
$fullOutputPath = Join-Path $fullOutputPath $OutputPath
$fullOutputPath = [System.IO.Path]::GetFullPath($fullOutputPath)
New-Item -ItemType Directory -Force -Path $fullOutputPath | Out-Null

# Check if already downloaded
$extractedPath = Join-Path $fullOutputPath "openvino_genai_windows_${Version}_x86_64"
if (Test-Path $extractedPath) {
    Write-Host "OpenVINO runtime already exists at: $extractedPath" -ForegroundColor Green
    Write-Host "Delete this directory to force re-download." -ForegroundColor Yellow
    exit 0
}


$baseUrl = "https://storage.openvinotoolkit.org/repositories/openvino_genai/packages"
$majorMinorVersion = "2025.2"  # Use the major.minor version for the URL path
$fileName = "openvino_genai_windows_${Version}_x86_64.zip"
$url = "$baseUrl/$majorMinorVersion/windows/$fileName"

Write-Host "Downloading from: $url" -ForegroundColor Yellow
$zipPath = Join-Path $fullOutputPath $fileName

try {
    # Download with simple approach
    Write-Host "Starting download..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing
    Write-Host "Download completed!" -ForegroundColor Green
    
    # Verify download
    $fileSize = (Get-Item $zipPath).Length
    Write-Host "Downloaded file size: $([math]::Round($fileSize/1MB, 2)) MB" -ForegroundColor Cyan
    
    # Extract using Expand-Archive
    Write-Host "Extracting archive..." -ForegroundColor Yellow
    Expand-Archive -Path $zipPath -DestinationPath $fullOutputPath -Force
    
    # Remove zip file
    Remove-Item $zipPath -Force
    
    # Copy DLLs to expected location
    $runtimePath = Join-Path $extractedPath "runtime"
    $targetPath = Join-Path $fullOutputPath "runtimes/win-x64/native"
    
    Write-Host "Setting up runtime DLLs..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $targetPath | Out-Null
    
    # Copy Release DLLs
    $releaseDlls = Join-Path $runtimePath "bin/intel64/Release/*.dll"
    Copy-Item $releaseDlls -Destination $targetPath -Force
    
    # Copy TBB DLLs
    $tbbDlls = Join-Path $runtimePath "3rdparty/tbb/bin/*.dll"
    Copy-Item $tbbDlls -Destination $targetPath -Force
    
    $dllCount = (Get-ChildItem $targetPath -Filter *.dll | Measure-Object).Count
    Write-Host "Copied $dllCount DLL files to: $targetPath" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "OpenVINO GenAI Runtime setup completed successfully!" -ForegroundColor Green
    Write-Host "Runtime location: $extractedPath" -ForegroundColor Cyan
    Write-Host "DLLs location: $targetPath" -ForegroundColor Cyan
    
} catch {
    Write-Host "Error downloading OpenVINO runtime: $_" -ForegroundColor Red
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    exit 1
}