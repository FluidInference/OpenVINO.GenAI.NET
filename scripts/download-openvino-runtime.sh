#!/bin/bash

# OpenVINO GenAI Runtime Downloader for Linux
# Downloads and sets up OpenVINO GenAI runtime for Linux x64

set -e

VERSION=${1:-"2025.3.0.0.dev20250801"}
OUTPUT_PATH=${2:-"build/native"}
UBUNTU_VERSION=${3:-"24"}

echo "OpenVINO GenAI Runtime Downloader"
echo "================================="
echo "Version: $VERSION"
echo "Output Path: $OUTPUT_PATH"
echo "Ubuntu Version: $UBUNTU_VERSION"
echo ""

# Get script directory and create output path
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FULL_OUTPUT_PATH="$(cd "$SCRIPT_DIR/.." && pwd)/$OUTPUT_PATH"
mkdir -p "$FULL_OUTPUT_PATH"
TARGET_PATH="$FULL_OUTPUT_PATH/runtimes/linux-x64/native"

export OPENVINO_RUNTIME_PATH="$TARGET_PATH"

# Check if already downloaded
EXTRACTED_PATH="$FULL_OUTPUT_PATH/openvino_genai_ubuntu${UBUNTU_VERSION}_${VERSION}_x86_64"
if [ -d "$EXTRACTED_PATH" ]; then
    echo "OpenVINO runtime already exists at: $EXTRACTED_PATH"
    echo "Delete this directory to force re-download."
    exit 0
fi

# Download URL  
BASE_URL="https://storage.openvinotoolkit.org/repositories/openvino_genai/packages/nightly/${VERSION}/"
FILE_NAME="openvino_genai_ubuntu${UBUNTU_VERSION}_${VERSION}_x86_64.tar.gz"
URL="$BASE_URL$FILE_NAME"

echo "Downloading from: $URL"
ZIP_PATH="$FULL_OUTPUT_PATH/$FILE_NAME"

# Download with progress
if command -v wget >/dev/null 2>&1; then
    wget --progress=bar "$URL" -O "$ZIP_PATH"
elif command -v curl >/dev/null 2>&1; then
    curl -# -L "$URL" -o "$ZIP_PATH"
else
    echo "Error: Neither wget nor curl is available for downloading"
    exit 1
fi

echo "Download completed!"

# Extract
echo "Extracting archive..."
tar -xzf "$ZIP_PATH" -C "$FULL_OUTPUT_PATH"

# Remove archive
rm -f "$ZIP_PATH"

# Copy libraries to expected location
RUNTIME_PATH="$EXTRACTED_PATH/runtime"

echo "Setting up runtime libraries..."
mkdir -p "$TARGET_PATH"

# 1️⃣  Copy OpenVINO core + plugins
find "$RUNTIME_PATH/lib" -type f -name '*.so*' -exec cp -a --no-preserve=ownership '{}' "$TARGET_PATH" \;

# 2️⃣  Copy TBB (handles lib/intel64/gcc layout too)
find "$RUNTIME_PATH/3rdparty/tbb/lib" -type f -name '*.so*' -exec cp -a --no-preserve=ownership '{}' "$TARGET_PATH" \;

# 3️⃣  Duplicate missing SONAME links if Intel ever forgets them
#     (e.g., create libopenvino.so.2520 → libopenvino.so.2025.2.0.0)
cd "$TARGET_PATH"
for so in *_genai.so *.so; do
  [[ $so =~ \.so\.([0-9]+\.[0-9]+\.[0-9]+) ]] || continue
  base="${so%%.so.*}.so"
  shortver="${BASH_REMATCH[1]//./}"   # 25.2.0 => 2520
  [[ -e "${base}.${shortver}" ]] || ln -s "$so" "${base}.${shortver}"
done
cd - >/dev/null

LIB_COUNT=$(find "$TARGET_PATH" -name "*.so" | wc -l)
echo "Copied $LIB_COUNT library files to: $TARGET_PATH"

echo "Setting OPENVINO_RUNTIME_PATH environment variable in ~/.bashrc"
echo "export OPENVINO_RUNTIME_PATH=$OPENVINO_RUNTIME_PATH" >> ~/.bashrc
source ~/.bashrc

echo ""
echo "OpenVINO GenAI Runtime setup completed successfully!"
echo "Runtime location: $EXTRACTED_PATH"
echo "Libraries location: $TARGET_PATH"