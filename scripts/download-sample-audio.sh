#!/bin/bash
#
# Downloads sample audio files for testing WhisperDemo
#

set -e

# Default values
OUTPUT_PATH="samples/audio"
FORCE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--path)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [-p OUTPUT_PATH] [-f]"
            echo "  -p, --path    Path where audio files should be downloaded (default: samples/audio)"
            echo "  -f, --force   Force re-download even if files exist"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Sample Audio Downloader"
echo "======================="
echo "Target Path: $OUTPUT_PATH"
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Create full path
FULL_OUTPUT_PATH="$PROJECT_ROOT/$OUTPUT_PATH"

# Create directory
echo "Creating directory: $FULL_OUTPUT_PATH"
mkdir -p "$FULL_OUTPUT_PATH"

# Define sample audio files to download
declare -a AUDIO_FILES=(
    "glm.wav|https://huggingface.co/datasets/FluidInference/audio/resolve/main/glm.wav|GLM audio sample from FluidInference"
    "startup.wav|https://huggingface.co/datasets/FluidInference/audio/resolve/main/startup.wav|Startup audio sample from FluidInference"
)

SUCCESS_COUNT=0

# Download each file
for AUDIO_INFO in "${AUDIO_FILES[@]}"; do
    IFS='|' read -r NAME URL DESCRIPTION <<< "$AUDIO_INFO"
    TARGET_PATH="$FULL_OUTPUT_PATH/$NAME"
    
    # Check if already exists
    if [ -f "$TARGET_PATH" ] && [ "$FORCE" = false ]; then
        echo "✓ $NAME already exists"
        SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
        continue
    fi
    
    echo "Downloading $NAME: $DESCRIPTION"
    
    # Download with retry logic
    MAX_RETRIES=3
    RETRY_COUNT=0
    DOWNLOADED=false
    
    while [ "$DOWNLOADED" = false ] && [ "$RETRY_COUNT" -lt "$MAX_RETRIES" ]; do
        if curl -L -H "User-Agent: Bash-AudioDownloader" \
                -o "$TARGET_PATH" \
                --silent --show-error \
                "$URL"; then
            
            # Verify file was downloaded
            if [ -f "$TARGET_PATH" ]; then
                FILE_SIZE=$(stat -f%z "$TARGET_PATH" 2>/dev/null || stat -c%s "$TARGET_PATH" 2>/dev/null || echo "0")
                if [ "$FILE_SIZE" -gt 1000 ]; then  # At least 1KB
                    # Convert to KB
                    FILE_SIZE_KB=$(echo "scale=2; $FILE_SIZE / 1024" | bc 2>/dev/null || echo "$((FILE_SIZE / 1024))")
                    echo "  ✓ Downloaded: ${FILE_SIZE_KB} KB"
                    DOWNLOADED=true
                    SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
                else
                    echo "  Downloaded file is too small"
                    rm -f "$TARGET_PATH"
                fi
            else
                echo "  File not found after download"
            fi
        fi
        
        if [ "$DOWNLOADED" = false ]; then
            RETRY_COUNT=$((RETRY_COUNT + 1))
            if [ "$RETRY_COUNT" -lt "$MAX_RETRIES" ]; then
                echo "  Retry $RETRY_COUNT/$MAX_RETRIES..."
                sleep 2
            else
                echo "  ✗ Error downloading $NAME"
                
                # Create a simple test WAV file as fallback
                echo "  Creating test audio file as fallback..."
                
                # Create a simple 1-second silence WAV file using dd
                if command -v python3 >/dev/null 2>&1; then
                    # Use Python to create a proper WAV file
                    python3 -c "
import struct
import sys

# WAV parameters
sample_rate = 16000
duration = 1
num_samples = sample_rate * duration

# WAV header
header = b'RIFF'
header += struct.pack('<I', 36 + num_samples * 2)  # File size - 8
header += b'WAVE'
header += b'fmt '
header += struct.pack('<I', 16)  # Subchunk1Size
header += struct.pack('<H', 1)   # AudioFormat (PCM)
header += struct.pack('<H', 1)   # NumChannels (mono)
header += struct.pack('<I', sample_rate)  # SampleRate
header += struct.pack('<I', sample_rate * 2)  # ByteRate
header += struct.pack('<H', 2)   # BlockAlign
header += struct.pack('<H', 16)  # BitsPerSample
header += b'data'
header += struct.pack('<I', num_samples * 2)  # Subchunk2Size

# Write to file
with open('$TARGET_PATH', 'wb') as f:
    f.write(header)
    # Write silence (zeros)
    f.write(b'\\x00' * (num_samples * 2))

print('  ✓ Created test audio file')
"
                    SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
                else
                    # Fallback: create empty file
                    touch "$TARGET_PATH"
                    echo "  ✓ Created placeholder file"
                fi
            fi
        fi
    done
done

echo ""
echo "Download completed!"
echo "Successfully downloaded/created $SUCCESS_COUNT of ${#AUDIO_FILES[@]} files"
echo "Audio files location: $FULL_OUTPUT_PATH"

# List downloaded files
echo ""
echo "Available audio files:"
for FILE in "$FULL_OUTPUT_PATH"/*.wav; do
    if [ -f "$FILE" ]; then
        BASENAME=$(basename "$FILE")
        FILE_SIZE=$(stat -f%z "$FILE" 2>/dev/null || stat -c%s "$FILE" 2>/dev/null || echo "0")
        FILE_SIZE_KB=$(echo "scale=2; $FILE_SIZE / 1024" | bc 2>/dev/null || echo "$((FILE_SIZE / 1024))")
        echo "  - $BASENAME (${FILE_SIZE_KB} KB)"
    fi
done