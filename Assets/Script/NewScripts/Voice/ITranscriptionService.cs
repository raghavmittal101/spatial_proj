using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// Interface for speech transcription services
/// </summary>
public interface ITranscriptionService
{
    /// <summary>
    /// Event triggered when transcription is complete
    /// </summary>
    event Action<string> TranscriptionCompleted;
    
    /// <summary>
    /// Event triggered when an error occurs during transcription
    /// </summary>
    event Action<string> TranscriptionError;
    
    /// <summary>
    /// Event triggered when transcription process starts
    /// </summary>
    event Action TranscriptionStarted;
    
    /// <summary>
    /// Configure the transcription service
    /// </summary>
    /// <param name="serverUrl">URL of the transcription server</param>
    void Configure(string serverUrl);
    
    /// <summary>
    /// Transcribe an audio clip asynchronously
    /// </summary>
    /// <param name="audioClip">The audio clip to transcribe</param>
    void TranscribeAudio(AudioClip audioClip);
    
    /// <summary>
    /// Check if transcription is in progress
    /// </summary>
    bool IsTranscribing { get; }
}