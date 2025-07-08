using System.Buffers;

namespace OpenVINO.NET.GenAI;

/// <summary>
/// Utility class for audio processing and format conversion for Whisper
/// </summary>
public static class AudioProcessor
{
    /// <summary>
    /// Target sample rate for Whisper (16kHz)
    /// </summary>
    public const int TargetSampleRate = 16000;

    /// <summary>
    /// Maximum audio duration in seconds (30 seconds for Whisper)
    /// </summary>
    public const int MaxDurationSeconds = 30;

    /// <summary>
    /// Maximum number of audio samples (30 seconds at 16kHz)
    /// </summary>
    public const int MaxSamples = TargetSampleRate * MaxDurationSeconds;

    /// <summary>
    /// Converts 16-bit PCM audio data to float32 format normalized to [-1.0, 1.0]
    /// </summary>
    /// <param name="pcmData">16-bit PCM audio data</param>
    /// <returns>Float32 normalized audio data</returns>
    public static float[] ConvertPcm16ToFloat32(short[] pcmData)
    {
        ArgumentNullException.ThrowIfNull(pcmData);

        var result = new float[pcmData.Length];
        for (int i = 0; i < pcmData.Length; i++)
        {
            result[i] = pcmData[i] / 32768.0f;
        }
        return result;
    }

    /// <summary>
    /// Converts 16-bit PCM audio data to float32 format normalized to [-1.0, 1.0]
    /// </summary>
    /// <param name="pcmData">16-bit PCM audio data as byte array</param>
    /// <returns>Float32 normalized audio data</returns>
    public static float[] ConvertPcm16ToFloat32(byte[] pcmData)
    {
        ArgumentNullException.ThrowIfNull(pcmData);

        if (pcmData.Length % 2 != 0)
            throw new ArgumentException("PCM data length must be even (16-bit samples)", nameof(pcmData));

        var result = new float[pcmData.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            result[i] = sample / 32768.0f;
        }
        return result;
    }

    /// <summary>
    /// Normalizes audio data to the range [-1.0, 1.0]
    /// </summary>
    /// <param name="audioData">Audio data to normalize</param>
    /// <returns>Normalized audio data</returns>
    public static float[] Normalize(float[] audioData)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        if (audioData.Length == 0)
            return Array.Empty<float>();

        // Find the maximum absolute value
        float maxAbsValue = 0.0f;
        for (int i = 0; i < audioData.Length; i++)
        {
            float absValue = Math.Abs(audioData[i]);
            if (absValue > maxAbsValue)
                maxAbsValue = absValue;
        }

        // If already normalized or silent, return as is
        if (maxAbsValue <= 1.0f)
            return audioData;

        // Normalize to [-1.0, 1.0]
        var result = new float[audioData.Length];
        for (int i = 0; i < audioData.Length; i++)
        {
            result[i] = audioData[i] / maxAbsValue;
        }
        return result;
    }

    /// <summary>
    /// Resamples audio from source sample rate to target sample rate (16kHz)
    /// Uses simple linear interpolation - for production use, consider more sophisticated resampling
    /// </summary>
    /// <param name="audioData">Source audio data</param>
    /// <param name="sourceSampleRate">Source sample rate in Hz</param>
    /// <param name="targetSampleRate">Target sample rate in Hz (default: 16000)</param>
    /// <returns>Resampled audio data</returns>
    public static float[] Resample(float[] audioData, int sourceSampleRate, int targetSampleRate = TargetSampleRate)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        if (sourceSampleRate <= 0)
            throw new ArgumentException("Source sample rate must be positive", nameof(sourceSampleRate));
        if (targetSampleRate <= 0)
            throw new ArgumentException("Target sample rate must be positive", nameof(targetSampleRate));

        if (sourceSampleRate == targetSampleRate)
            return audioData;

        double ratio = (double)sourceSampleRate / targetSampleRate;
        int outputLength = (int)(audioData.Length / ratio);
        var result = new float[outputLength];

        for (int i = 0; i < outputLength; i++)
        {
            double sourceIndex = i * ratio;
            int leftIndex = (int)sourceIndex;
            int rightIndex = Math.Min(leftIndex + 1, audioData.Length - 1);

            if (leftIndex == rightIndex)
            {
                result[i] = audioData[leftIndex];
            }
            else
            {
                double fraction = sourceIndex - leftIndex;
                result[i] = (float)((1.0 - fraction) * audioData[leftIndex] + fraction * audioData[rightIndex]);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts stereo audio to mono by averaging the channels
    /// </summary>
    /// <param name="stereoData">Stereo audio data (interleaved left/right)</param>
    /// <returns>Mono audio data</returns>
    public static float[] StereoToMono(float[] stereoData)
    {
        ArgumentNullException.ThrowIfNull(stereoData);

        if (stereoData.Length % 2 != 0)
            throw new ArgumentException("Stereo data length must be even", nameof(stereoData));

        var result = new float[stereoData.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            float left = stereoData[i * 2];
            float right = stereoData[i * 2 + 1];
            result[i] = (left + right) * 0.5f;
        }
        return result;
    }

    /// <summary>
    /// Trims silence from the beginning and end of audio data
    /// </summary>
    /// <param name="audioData">Audio data to trim</param>
    /// <param name="threshold">Silence threshold (default: 0.01)</param>
    /// <returns>Trimmed audio data</returns>
    public static float[] TrimSilence(float[] audioData, float threshold = 0.01f)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        if (audioData.Length == 0)
            return Array.Empty<float>();

        // Find start of non-silence
        int start = 0;
        while (start < audioData.Length && Math.Abs(audioData[start]) < threshold)
            start++;

        // Find end of non-silence
        int end = audioData.Length - 1;
        while (end >= start && Math.Abs(audioData[end]) < threshold)
            end--;

        if (start > end)
            return Array.Empty<float>();

        int length = end - start + 1;
        var result = new float[length];
        Array.Copy(audioData, start, result, 0, length);
        return result;
    }

    /// <summary>
    /// Pads or truncates audio data to the specified length
    /// </summary>
    /// <param name="audioData">Audio data to pad or truncate</param>
    /// <param name="targetLength">Target length in samples</param>
    /// <param name="padValue">Value to use for padding (default: 0.0)</param>
    /// <returns>Padded or truncated audio data</returns>
    public static float[] PadOrTruncate(float[] audioData, int targetLength, float padValue = 0.0f)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        if (targetLength <= 0)
            throw new ArgumentException("Target length must be positive", nameof(targetLength));

        if (audioData.Length == targetLength)
            return audioData;

        var result = new float[targetLength];

        if (audioData.Length > targetLength)
        {
            // Truncate
            Array.Copy(audioData, result, targetLength);
        }
        else
        {
            // Pad
            Array.Copy(audioData, result, audioData.Length);
            if (padValue != 0.0f)
            {
                for (int i = audioData.Length; i < targetLength; i++)
                {
                    result[i] = padValue;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Validates audio data format and constraints for Whisper
    /// </summary>
    /// <param name="audioData">Audio data to validate</param>
    /// <param name="sampleRate">Sample rate of the audio data</param>
    /// <exception cref="ArgumentException">Thrown when audio data is invalid</exception>
    public static void ValidateAudioData(float[] audioData, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        if (audioData.Length == 0)
            throw new ArgumentException("Audio data cannot be empty", nameof(audioData));

        if (sampleRate != TargetSampleRate)
            throw new ArgumentException($"Audio must be {TargetSampleRate}Hz, got {sampleRate}Hz", nameof(sampleRate));

        if (audioData.Length > MaxSamples)
            throw new ArgumentException($"Audio is too long. Maximum length is {MaxSamples} samples ({MaxDurationSeconds} seconds)", nameof(audioData));

        // Check for invalid values
        for (int i = 0; i < audioData.Length; i++)
        {
            if (float.IsNaN(audioData[i]) || float.IsInfinity(audioData[i]))
                throw new ArgumentException($"Audio data contains invalid value at index {i}: {audioData[i]}", nameof(audioData));
        }
    }

    /// <summary>
    /// Prepares audio data for Whisper processing (resample, normalize, validate)
    /// </summary>
    /// <param name="audioData">Raw audio data</param>
    /// <param name="sourceSampleRate">Source sample rate</param>
    /// <param name="normalize">Whether to normalize the audio (default: true)</param>
    /// <returns>Processed audio data ready for Whisper</returns>
    public static float[] PrepareForWhisper(float[] audioData, int sourceSampleRate, bool normalize = true)
    {
        ArgumentNullException.ThrowIfNull(audioData);

        // Resample to 16kHz if needed
        var processedAudio = Resample(audioData, sourceSampleRate, TargetSampleRate);

        // Normalize if requested
        if (normalize)
        {
            processedAudio = Normalize(processedAudio);
        }

        // Validate the result
        ValidateAudioData(processedAudio, TargetSampleRate);

        return processedAudio;
    }
}