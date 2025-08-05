<#
.SYNOPSIS
    Downloads sample audio files for testing WhisperDemo
.DESCRIPTION
    This script downloads free sample WAV files containing speech for testing speech recognition.
.PARAMETER OutputPath
    The path where audio files should be downloaded (default: samples/audio)
.PARAMETER Force
    Force re-download even if files exist
.EXAMPLE
    .\download-sample-audio.ps1
.EXAMPLE
    .\download-sample-audio.ps1 -OutputPath "custom/path" -Force
#>

param(
    [string]$OutputPath = "samples/audio",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "Sample Audio Downloader" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan
Write-Host "Target Path: $OutputPath"
Write-Host ""

# Create full path
$fullOutputPath = Join-Path $PSScriptRoot ".."
$fullOutputPath = Join-Path $fullOutputPath $OutputPath
$fullOutputPath = [System.IO.Path]::GetFullPath($fullOutputPath)

# Create directory
Write-Host "Creating directory: $fullOutputPath" -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $fullOutputPath | Out-Null

# Define sample audio files to download
$audioFiles = @(
    @{
        Name = "glm.wav"
        Url = "https://huggingface.co/datasets/FluidInference/audio/resolve/main/glm.wav"
        Description = "GLM audio sample from FluidInference"
    },
    @{
        Name = "startup.wav"
        Url = "https://huggingface.co/datasets/FluidInference/audio/resolve/main/startup.wav"
        Description = "Startup audio sample from FluidInference"
    }
)

$successCount = 0

foreach ($audio in $audioFiles) {
    $targetPath = Join-Path $fullOutputPath $audio.Name
    
    # Check if already exists
    if ((Test-Path $targetPath) -and -not $Force) {
        Write-Host "✓ $($audio.Name) already exists" -ForegroundColor Green
        $successCount++
        continue
    }
    
    Write-Host "Downloading $($audio.Name): $($audio.Description)" -ForegroundColor Yellow
    
    try {
        # Download with retry logic
        $maxRetries = 3
        $retryCount = 0
        $downloaded = $false
        
        while (-not $downloaded -and $retryCount -lt $maxRetries) {
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.Headers.Add("User-Agent", "PowerShell-AudioDownloader")
                $webClient.DownloadFile($audio.Url, $targetPath)
                
                # Verify file was downloaded
                if (Test-Path $targetPath) {
                    $fileSize = (Get-Item $targetPath).Length
                    if ($fileSize -gt 1000) {  # At least 1KB
                        Write-Host "  ✓ Downloaded: $([math]::Round($fileSize/1024, 2)) KB" -ForegroundColor Green
                        $downloaded = $true
                        $successCount++
                    } else {
                        throw "Downloaded file is too small"
                    }
                } else {
                    throw "File not found after download"
                }
            } catch {
                $retryCount++
                if ($retryCount -lt $maxRetries) {
                    Write-Host "  Retry $retryCount/$maxRetries..." -ForegroundColor Yellow
                    Start-Sleep -Seconds 2
                } else {
                    throw $_
                }
            }
        }
    } catch {
        Write-Host "  ✗ Error downloading $($audio.Name): $_" -ForegroundColor Red
        
        # Create a simple test WAV file as fallback
        Write-Host "  Creating test audio file as fallback..." -ForegroundColor Yellow
        
        try {
            # Create a simple 1-second silence WAV file
            $sampleRate = 16000
            $duration = 1
            $numSamples = $sampleRate * $duration
            
            # WAV file header
            $header = [byte[]]@(
                0x52, 0x49, 0x46, 0x46,  # "RIFF"
                0x00, 0x00, 0x00, 0x00,  # File size (will update)
                0x57, 0x41, 0x56, 0x45,  # "WAVE"
                0x66, 0x6D, 0x74, 0x20,  # "fmt "
                0x10, 0x00, 0x00, 0x00,  # Subchunk1Size (16 for PCM)
                0x01, 0x00,              # AudioFormat (1 for PCM)
                0x01, 0x00,              # NumChannels (1 for mono)
                0x80, 0x3E, 0x00, 0x00,  # SampleRate (16000)
                0x00, 0x7D, 0x00, 0x00,  # ByteRate (32000)
                0x02, 0x00,              # BlockAlign (2)
                0x10, 0x00,              # BitsPerSample (16)
                0x64, 0x61, 0x74, 0x61,  # "data"
                0x00, 0x00, 0x00, 0x00   # Subchunk2Size (will update)
            )
            
            # Calculate sizes
            $dataSize = $numSamples * 2  # 16-bit samples
            $fileSize = $header.Length + $dataSize - 8
            
            # Update size fields
            [System.BitConverter]::GetBytes([int]$fileSize).CopyTo($header, 4)
            [System.BitConverter]::GetBytes([int]$dataSize).CopyTo($header, 40)
            
            # Write WAV file
            $fs = [System.IO.File]::Create($targetPath)
            $fs.Write($header, 0, $header.Length)
            
            # Write silence (zeros)
            $silence = New-Object byte[] $dataSize
            $fs.Write($silence, 0, $silence.Length)
            $fs.Close()
            
            Write-Host "  ✓ Created test audio file" -ForegroundColor Green
            $successCount++
        } catch {
            Write-Host "  ✗ Failed to create test audio: $_" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Download completed!" -ForegroundColor Green
Write-Host "Successfully downloaded/created $successCount of $($audioFiles.Count) files" -ForegroundColor Cyan
Write-Host "Audio files location: $fullOutputPath" -ForegroundColor Cyan

# List downloaded files
$downloadedFiles = Get-ChildItem -Path $fullOutputPath -Filter "*.wav" -ErrorAction SilentlyContinue
if ($downloadedFiles) {
    Write-Host ""
    Write-Host "Available audio files:" -ForegroundColor Cyan
    foreach ($file in $downloadedFiles) {
        Write-Host "  - $($file.Name) ($([math]::Round($file.Length/1024, 2)) KB)" -ForegroundColor Gray
    }
}