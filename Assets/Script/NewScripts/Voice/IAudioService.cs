using UnityEngine;
using System;

/// <summary>
/// Interface for audio recording services
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Start recording audio
    /// </summary>
    /// <param name="maxRecordingDuration">Maximum duration to record in seconds</param>
    /// <param name="frequency">Recording frequency in Hz</param>
    /// <returns>True if recording started successfully</returns>
    bool StartRecording(int maxRecordingDuration, int frequency);
    
    /// <summary>
    /// Stop recording and get the recorded audio clip
    /// </summary>
    /// <returns>The recorded AudioClip</returns>
    AudioClip StopRecording();
    
    /// <summary>
    /// Get the current recording progress from 0 to 1
    /// </summary>
    /// <returns>Recording progress as a value between 0 and 1</returns>
    float GetRecordingProgress();
    
    /// <summary>
    /// Get list of available audio devices
    /// </summary>
    /// <returns>Array of device names</returns>
    string[] GetAvailableDevices();
    
    /// <summary>
    /// Set the audio device to use for recording
    /// </summary>
    /// <param name="deviceName">Name of the device to use</param>
    void SetAudioDevice(string deviceName);
    
    /// <summary>
    /// Check if currently recording
    /// </summary>
    bool IsRecording { get; }
}