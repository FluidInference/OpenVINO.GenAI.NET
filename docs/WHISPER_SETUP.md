# Running WhisperDemo Locally

## Requirements

1. **OpenVINO GenAI 2025.3+**: Required for Whisper C API support
   - Earlier versions (2025.2 and below) do NOT have Whisper C API support
   - Must have libraries built with Whisper support enabled

2. **Whisper Model**: OpenVINO IR format model
   - Use models converted with OpenVINO Model Converter
   - FP16 models work on CPU/GPU
   - INT4 models are NPU-specific

## Setup Instructions

### Step 1: Get OpenVINO GenAI 2025.3+ Libraries

Option A: Build from source (recommended for latest Whisper support):
```bash
git clone https://github.com/openvinotoolkit/openvino.genai.git
cd openvino.genai
cmake -B build -DENABLE_WHISPER=ON
cmake --build build --config Release
```

Option B: Download nightly build (check for Whisper support):
```bash
./scripts/download-openvino-runtime.sh "2025.0.0.0.dev20250106" "build/native" "24"
```

### Step 2: Download Whisper Model

```bash
# Download Whisper Tiny model (for testing)
./scripts/download-whisper-model.sh

# Or use a pre-converted model from OpenVINO GenAI tests
cp -r /path/to/openvino.genai/ov_cache0/test_models/WhisperTiny/openai/whisper-tiny Models/
```

### Step 3: Set Up Libraries

```bash
# Copy the 2025.3+ libraries to the sample directory
cp /path/to/openvino.genai/build/src/c/libopenvino_genai_c.so.2025.3.0.0 \
   samples/WhisperDemo/bin/Debug/net8.0/runtimes/linux-x64/native/

cp /path/to/openvino.genai/build/openvino_genai/libopenvino_genai.so.2025.3.0.0 \
   samples/WhisperDemo/bin/Debug/net8.0/runtimes/linux-x64/native/

# Copy OpenVINO runtime dependencies
cp /path/to/openvino.genai/.venv/lib/python3.12/site-packages/openvino/libs/*.so* \
   samples/WhisperDemo/bin/Debug/net8.0/runtimes/linux-x64/native/

# Create symlinks
cd samples/WhisperDemo/bin/Debug/net8.0/runtimes/linux-x64/native
ln -sf libopenvino_genai_c.so.2025.3.0.0 libopenvino_genai_c.so
ln -sf libopenvino_genai.so.2025.3.0.0 libopenvino_genai.so
cd -
```

### Step 4: Run WhisperDemo

```bash
# Build the solution
dotnet build OpenVINO.NET.sln

# Navigate to the sample directory
cd samples/WhisperDemo/bin/Debug/net8.0

# Run with proper library paths (critical!)
LD_LIBRARY_PATH=/path/to/openvino.genai/build/src/c:/path/to/openvino.genai/build/openvino_genai:/path/to/openvino.genai/.venv/lib/python3.12/site-packages/openvino/libs \
dotnet WhisperDemo.dll --audio=/path/to/audio.wav --device=CPU
```

## Working Example

Using the test audio from OpenVINO GenAI:
```bash
cd samples/WhisperDemo/bin/Debug/net8.0

LD_LIBRARY_PATH=/home/brandon/openvino.genai/build/src/c:/home/brandon/openvino.genai/build/openvino_genai:/home/brandon/openvino.genai/.venv/lib/python3.12/site-packages/openvino/libs \
dotnet WhisperDemo.dll --audio=/home/brandon/openvino.genai/ov_cache0/test_data/how_are_you_doing_today.wav --device=CPU
```

Expected output:
```
OpenVINO.NET Whisper Demo
=========================

Transcribing: /home/brandon/openvino.genai/ov_cache0/test_data/how_are_you_doing_today.wav
Device: CPU
Task: Transcribe
Timestamps: No

Transcription completed in 523.45ms
=====================================
 How are you doing today?
=====================================
```

## Troubleshooting

### Error -17: Model Not Found
- Ensure model directory contains all required files (encoder, decoder, tokenizer)
- Check model path is absolute or relative to current directory
- Verify model is in OpenVINO IR format

### DLL/Library Not Found
- Ensure OPENVINO_RUNTIME_PATH is not set (or points to 2025.3+ libraries)
- Use LD_LIBRARY_PATH to specify exact library locations
- Check library versions with: `nm -D libopenvino_genai_c.so | grep whisper`

### Segmentation Fault
- Library version mismatch - ensure all libraries are from same build
- Missing dependencies - check with `ldd libopenvino_genai_c.so`

### Audio Format Issues
- Currently only supports PCM WAV files (16-bit, mono/stereo)
- Sample rate will be resampled to 16kHz automatically
- For other formats, convert first or use NAudio library

## Notes

- INT4 quantized models are NPU-specific and won't work on CPU
- FP16 models provide best compatibility across devices
- The Whisper C API is relatively new (2025.3+) and may have limited documentation