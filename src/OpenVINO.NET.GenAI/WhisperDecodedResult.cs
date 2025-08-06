namespace Fluid.OpenVINO.GenAI;

/// <summary>
/// Represents a decoded result from Whisper speech recognition
/// </summary>
public sealed class WhisperDecodedResult
{
    /// <summary>
    /// Gets the transcribed text
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the confidence score for this result
    /// </summary>
    public float Score { get; }

    /// <summary>
    /// Gets the timestamped chunks if available
    /// </summary>
    public IReadOnlyList<WhisperChunk>? Chunks { get; }

    /// <summary>
    /// Initializes a new instance of the WhisperDecodedResult class
    /// </summary>
    /// <param name="text">The transcribed text</param>
    /// <param name="score">The confidence score</param>
    /// <param name="chunks">Optional timestamped chunks</param>
    public WhisperDecodedResult(string text, float score, IReadOnlyList<WhisperChunk>? chunks = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Score = score;
        Chunks = chunks;
    }

    /// <summary>
    /// Gets a value indicating whether this result has timestamped chunks
    /// </summary>
    public bool HasChunks => Chunks != null && Chunks.Count > 0;
}

/// <summary>
/// Represents a timestamped chunk of transcribed text
/// </summary>
public sealed class WhisperChunk
{
    /// <summary>
    /// Gets the start timestamp in seconds
    /// </summary>
    public float StartTime { get; }

    /// <summary>
    /// Gets the end timestamp in seconds
    /// </summary>
    public float EndTime { get; }

    /// <summary>
    /// Gets the transcribed text for this chunk
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of the WhisperChunk class
    /// </summary>
    /// <param name="startTime">Start timestamp in seconds</param>
    /// <param name="endTime">End timestamp in seconds</param>
    /// <param name="text">Transcribed text</param>
    public WhisperChunk(float startTime, float endTime, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>
    /// Gets the duration of this chunk in seconds
    /// </summary>
    public float Duration => EndTime - StartTime;

    /// <summary>
    /// Returns a string representation of this chunk
    /// </summary>
    public override string ToString() => $"[{StartTime:F2}s - {EndTime:F2}s]: {Text}";
}