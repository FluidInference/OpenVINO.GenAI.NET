namespace OpenVINO.NET.GenAI;

/// <summary>
/// Utility class for audio processing
/// </summary>
public static class AudioUtils
{
    private const int WhisperSampleRate = 16000;

    /// <summary>
    /// Loads audio from a file and converts it to the format expected by Whisper
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <returns>Audio data as float array (16kHz, mono, normalized to [-1, 1])</returns>
    public static float[] LoadAudioFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {filePath}");

        // For now, we'll provide a simple WAV file loader
        // In a production implementation, you might want to use a library like NAudio
        if (Path.GetExtension(filePath).ToLowerInvariant() != ".wav")
        {
            throw new NotSupportedException("Currently only WAV files are supported. Consider using a library like NAudio for other formats.");
        }

        return LoadWavFile(filePath);
    }

    /// <summary>
    /// Loads audio from a file asynchronously
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audio data as float array (16kHz, mono, normalized to [-1, 1])</returns>
    public static async Task<float[]> LoadAudioFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // For now, just run the synchronous version on a background thread
        return await Task.Run(() => LoadAudioFile(filePath), cancellationToken);
    }

    /// <summary>
    /// Creates audio data from a byte array (assumes 16-bit PCM, 16kHz, mono)
    /// </summary>
    /// <param name="pcmData">Raw PCM data as bytes</param>
    /// <returns>Audio data as float array normalized to [-1, 1]</returns>
    public static float[] FromPcm16(byte[] pcmData)
    {
        if (pcmData == null)
            throw new ArgumentNullException(nameof(pcmData));
        if (pcmData.Length % 2 != 0)
            throw new ArgumentException("PCM data length must be even (16-bit samples)", nameof(pcmData));

        var samples = new float[pcmData.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            // Convert 16-bit PCM to float [-1, 1]
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            samples[i] = sample / 32768.0f;
        }

        return samples;
    }

    /// <summary>
    /// Creates audio data from a float array (assumes already at correct sample rate)
    /// </summary>
    /// <param name="floatData">Audio samples as floats</param>
    /// <returns>Audio data normalized to [-1, 1]</returns>
    public static float[] NormalizeAudio(float[] floatData)
    {
        if (floatData == null)
            throw new ArgumentNullException(nameof(floatData));

        var normalized = new float[floatData.Length];

        // Find max absolute value
        float maxAbs = 0;
        for (int i = 0; i < floatData.Length; i++)
        {
            float abs = Math.Abs(floatData[i]);
            if (abs > maxAbs)
                maxAbs = abs;
        }

        // Normalize if needed
        if (maxAbs > 0 && maxAbs != 1.0f)
        {
            float scale = 1.0f / maxAbs;
            for (int i = 0; i < floatData.Length; i++)
            {
                normalized[i] = floatData[i] * scale;
            }
        }
        else
        {
            Array.Copy(floatData, normalized, floatData.Length);
        }

        return normalized;
    }

    private static float[] LoadWavFile(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream);

        // Read WAV header
        var chunkId = new string(reader.ReadChars(4));
        if (chunkId != "RIFF")
            throw new InvalidOperationException("Not a valid WAV file - missing RIFF header");

        reader.ReadInt32(); // chunk size
        var format = new string(reader.ReadChars(4));
        if (format != "WAVE")
            throw new InvalidOperationException("Not a valid WAV file - missing WAVE format");

        // Find fmt chunk
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var subchunkId = new string(reader.ReadChars(4));
            var subchunkSize = reader.ReadInt32();

            if (subchunkId == "fmt ")
            {
                var audioFormat = reader.ReadInt16();
                if (audioFormat != 1) // PCM
                    throw new NotSupportedException("Only PCM WAV files are supported");

                var channels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadInt16(); // block align
                var bitsPerSample = reader.ReadInt16();

                if (bitsPerSample != 16)
                    throw new NotSupportedException("Only 16-bit WAV files are supported");

                // Skip rest of fmt chunk if any
                reader.BaseStream.Seek(subchunkSize - 16, SeekOrigin.Current);
            }
            else if (subchunkId == "data")
            {
                // Read audio data
                var audioData = reader.ReadBytes(subchunkSize);
                return FromPcm16(audioData);
            }
            else
            {
                // Skip unknown chunk
                reader.BaseStream.Seek(subchunkSize, SeekOrigin.Current);
            }
        }

        throw new InvalidOperationException("No audio data found in WAV file");
    }
}