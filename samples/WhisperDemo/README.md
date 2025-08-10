# Whisper Demo

This sample demonstrates using the OpenVINO.NET Whisper pipeline for speech recognition and translation.

## Prerequisites

1. Download a Whisper model converted for OpenVINO and place it in the `models/` directory
2. Prepare audio files in WAV format (16kHz, mono recommended)

## Usage

Download a model from: https://huggingface.co/collections/FluidInference/openvino-npu-6877c0392d32707ed3e7bf35

### Basic transcription
```bash
 dotnet run --project .\samples\WhisperDemo\ .\Models\whisper-large-v3-turbo-fp16-ov-npu\   how_are_you_doing_today.wav
```

```bash
 dotnet run --project .\samples\WhisperDemo\ .\Models\whisper-large-v3-turbo-fp16-ov-npu\   how_are_you_doing_today.wav  NPU
 ```


## Command Line Options

- `--device=<DEVICE>` - Inference device (CPU, GPU, NPU). Default: CPU
- `--audio=<PATH>` - Path to audio file to transcribe
- `--audio-dir=<PATH>` - Path to directory containing audio files
- `--language=<LANG>` - Source language code (e.g., en, es, fr). Default: en
- `--translate` - Translate to English instead of transcribing
- `--timestamps` - Include word-level timestamps in output
- `--benchmark` - Run performance benchmark
- `--workflow` - CI/CD friendly output format (no headers, structured output)

## Model Download

You need to download a Whisper model that has been converted to OpenVINO format. You can:

1. Use the OpenVINO Model Converter to convert a Whisper model from Hugging Face
2. Download a pre-converted model (if available)

Example conversion command:
```bash
omz_converter --name whisper-tiny-en --download_dir models/ --output_dir models/
```

## Audio Format

The demo supports WAV files. For best results:
- Sample rate: 16kHz
- Channels: Mono
- Bit depth: 16-bit PCM

For other audio formats, consider using a library like NAudio to convert to the required format.