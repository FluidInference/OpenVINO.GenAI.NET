<#
.SYNOPSIS
    Downloads the Whisper FP16 model for OpenVINO from HuggingFace
.DESCRIPTION
    This script downloads the OpenVINO/whisper-tiny-fp16-ov model.
    This is a standard FP16 precision model that works well on CPU/GPU.
.PARAMETER ModelPath
    The path where the model should be downloaded (default: Models/whisper-tiny-fp16-ov)
.PARAMETER Force
    Force re-download even if model exists
.EXAMPLE
    .\download-whisper-fp16-model.ps1
.EXAMPLE
    .\download-whisper-fp16-model.ps1 -ModelPath "custom/path" -Force
#>

param(
    [string]$ModelPath = "Models/whisper-tiny-fp16-ov",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "Whisper FP16 Model Downloader" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Model: OpenVINO/whisper-tiny-fp16-ov"
Write-Host "Target Path: $ModelPath"
Write-Host ""

# Create full path
$fullModelPath = Join-Path $PSScriptRoot ".."
$fullModelPath = Join-Path $fullModelPath $ModelPath
$fullModelPath = [System.IO.Path]::GetFullPath($fullModelPath)

# Check if already downloaded
if ((Test-Path $fullModelPath) -and -not $Force) {
    $encoderPath = Join-Path $fullModelPath "openvino_encoder_model.xml"
    if (Test-Path $encoderPath) {
        Write-Host "Model already exists at: $fullModelPath" -ForegroundColor Green
        Write-Host "Use -Force to re-download." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Creating directory: $fullModelPath" -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $fullModelPath | Out-Null

# Files to download
$baseUrl = "https://huggingface.co/OpenVINO/whisper-tiny-fp16-ov/resolve/main"
$files = @(
    "openvino_encoder_model.xml",
    "openvino_encoder_model.bin",
    "openvino_decoder_model.xml",
    "openvino_decoder_model.bin",
    "openvino_tokenizer.xml",
    "openvino_tokenizer.bin",
    "openvino_detokenizer.xml",
    "openvino_detokenizer.bin",
    "config.json",
    "generation_config.json",
    "preprocessor_config.json",
    "tokenizer.json",
    "tokenizer_config.json",
    "added_tokens.json",
    "merges.txt",
    "normalizer.json",
    "special_tokens_map.json",
    "vocab.json"
)

$totalFiles = $files.Count
$currentFile = 0

foreach ($file in $files) {
    $currentFile++
    $url = "$baseUrl/$file"
    $targetPath = Join-Path $fullModelPath $file
    
    Write-Host "[$currentFile/$totalFiles] Downloading $file..." -ForegroundColor Yellow
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell-HuggingFace-Downloader")
        
        # Download with progress
        $webClient.DownloadFile($url, $targetPath)
        
        # Verify file was downloaded
        if (Test-Path $targetPath) {
            $fileSize = (Get-Item $targetPath).Length
            if ($fileSize -gt 0) {
                Write-Host "  Downloaded: $([math]::Round($fileSize/1MB, 2)) MB" -ForegroundColor Green
            } else {
                throw "Downloaded file is empty"
            }
        } else {
            throw "Failed to download file"
        }
    } catch {
        Write-Host "  Error downloading $file : $_" -ForegroundColor Red
        
        # Cleanup partial download
        if (Test-Path $targetPath) {
            Remove-Item $targetPath -Force
        }
        
        # Exit with error
        exit 1
    }
}

Write-Host ""
Write-Host "Model downloaded successfully!" -ForegroundColor Green
Write-Host "Location: $fullModelPath" -ForegroundColor Cyan

# Verify critical files
$criticalFiles = @(
    "openvino_encoder_model.xml",
    "openvino_encoder_model.bin",
    "openvino_decoder_model.xml",
    "openvino_decoder_model.bin"
)
foreach ($file in $criticalFiles) {
    $path = Join-Path $fullModelPath $file
    if (-not (Test-Path $path)) {
        Write-Host "Warning: Critical file missing: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Model verification passed!" -ForegroundColor Green