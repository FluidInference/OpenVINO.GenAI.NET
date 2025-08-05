#!/bin/bash
#
# Downloads the Whisper model for OpenVINO from HuggingFace
# Model: FluidInference/whisper-tiny-int4-ov-npu
#

set -e

# Default values
MODEL_PATH="Models/whisper-tiny-int4-ov-npu"
FORCE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--path)
            MODEL_PATH="$2"
            shift 2
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [-p MODEL_PATH] [-f]"
            echo "  -p, --path    Path where model should be downloaded (default: Models/whisper-tiny-int4-ov-npu)"
            echo "  -f, --force   Force re-download even if model exists"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Whisper Model Downloader"
echo "========================"
echo "Model: FluidInference/whisper-tiny-int4-ov-npu"
echo "Target Path: $MODEL_PATH"
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Create full path
FULL_MODEL_PATH="$PROJECT_ROOT/$MODEL_PATH"

# Check if already downloaded
if [ -d "$FULL_MODEL_PATH" ] && [ "$FORCE" = false ]; then
    if [ -f "$FULL_MODEL_PATH/openvino_encoder_model.xml" ]; then
        echo "Model already exists at: $FULL_MODEL_PATH"
        echo "Use -f/--force to re-download."
        exit 0
    fi
fi

echo "Creating directory: $FULL_MODEL_PATH"
mkdir -p "$FULL_MODEL_PATH"

# Base URL for HuggingFace
BASE_URL="https://huggingface.co/FluidInference/whisper-tiny-int4-ov-npu/resolve/main"

# Files to download
FILES=(
    "openvino_encoder_model.xml"
    "openvino_encoder_model.bin"
    "openvino_decoder_model.xml"
    "openvino_decoder_model.bin"
    "openvino_tokenizer.xml"
    "openvino_tokenizer.bin"
    "openvino_detokenizer.xml"
    "openvino_detokenizer.bin"
    "config.json"
    "generation_config.json"
    "preprocessor_config.json"
    "tokenizer.json"
    "tokenizer_config.json"
    "added_tokens.json"
    "merges.txt"
    "normalizer.json"
    "special_tokens_map.json"
    "vocab.json"
)

TOTAL_FILES=${#FILES[@]}
CURRENT_FILE=0

# Download each file
for FILE in "${FILES[@]}"; do
    CURRENT_FILE=$((CURRENT_FILE + 1))
    URL="$BASE_URL/$FILE"
    TARGET_PATH="$FULL_MODEL_PATH/$FILE"
    
    echo "[$CURRENT_FILE/$TOTAL_FILES] Downloading $FILE..."
    
    # Download file
    if curl -L -H "User-Agent: Bash-HuggingFace-Downloader" \
            -o "$TARGET_PATH" \
            --progress-bar \
            "$URL"; then
        
        # Verify file was downloaded
        if [ -f "$TARGET_PATH" ]; then
            FILE_SIZE=$(stat -f%z "$TARGET_PATH" 2>/dev/null || stat -c%s "$TARGET_PATH" 2>/dev/null || echo "0")
            if [ "$FILE_SIZE" -gt 0 ]; then
                # Convert to MB
                FILE_SIZE_MB=$(echo "scale=2; $FILE_SIZE / 1048576" | bc 2>/dev/null || echo "N/A")
                echo "  Downloaded: ${FILE_SIZE_MB} MB"
            else
                echo "  Error: Downloaded file is empty"
                rm -f "$TARGET_PATH"
                exit 1
            fi
        else
            echo "  Error: Failed to download file"
            exit 1
        fi
    else
        echo "  Error downloading $FILE"
        rm -f "$TARGET_PATH"
        exit 1
    fi
done

echo ""
echo "Model downloaded successfully!"
echo "Location: $FULL_MODEL_PATH"

# Verify critical files
CRITICAL_FILES=(
    "openvino_encoder_model.xml"
    "openvino_encoder_model.bin"
    "openvino_decoder_model.xml"
    "openvino_decoder_model.bin"
)
for FILE in "${CRITICAL_FILES[@]}"; do
    if [ ! -f "$FULL_MODEL_PATH/$FILE" ]; then
        echo "Error: Critical file missing: $FILE"
        exit 1
    fi
done

echo "Model verification passed!"