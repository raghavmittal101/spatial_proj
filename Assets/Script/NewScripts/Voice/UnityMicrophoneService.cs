using UnityEngine;
using System;

/// <summary>
/// Implementation of IAudioService using Unity's Microphone API
/// </summary>
public class UnityMicrophoneService : MonoBehaviour, IAudioService
{
    private string _currentDeviceName;
    private AudioClip _currentRecording;
    private bool _isRecording;
    
    private void Awake()
    {
        // Default to the first available device if there are any
        if (Microphone.devices.Length > 0)
        {
            _currentDeviceName = Microphone.devices[0];
        }
    }
    
    /// <summary>
    /// Start recording using the currently selected microphone device
    /// </summary>
    public bool StartRecording(int maxRecordingDuration, int frequency)
    {
        if (string.IsNullOrEmpty(_currentDeviceName))
        {
            Debug.LogError("No microphone device selected");
            return false;
        }
        
        try
        {
            _currentRecording = Microphone.Start(_currentDeviceName, false, maxRecordingDuration, frequency);
            _isRecording = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start recording: {ex.Message}");
            _isRecording = false;
            return false;
        }
    }
    
    /// <summary>
    /// Stop the current recording and return the AudioClip
    /// </summary>
    public AudioClip StopRecording()
    {
        if (!_isRecording)
        {
            Debug.LogWarning("Attempted to stop recording when not recording");
            return null;
        }
        
        Microphone.End(_currentDeviceName);
        _isRecording = false;
        return _currentRecording;
    }
    
    /// <summary>
    /// Get current recording progress from 0 to 1
    /// </summary>
    public float GetRecordingProgress()
    {
        if (!_isRecording || _currentRecording == null)
        {
            return 0f;
        }
        
        return (float)Microphone.GetPosition(_currentDeviceName) / _currentRecording.samples;
    }
    
    /// <summary>
    /// Get all available microphone devices
    /// </summary>
    public string[] GetAvailableDevices()
    {
        return Microphone.devices;
    }
    
    /// <summary>
    /// Set the microphone device to use for recording
    /// </summary>
    public void SetAudioDevice(string deviceName)
    {
        if (_isRecording)
        {
            Debug.LogWarning("Cannot change audio device while recording");
            return;
        }
        
        if (Array.IndexOf(Microphone.devices, deviceName) >= 0)
        {
            _currentDeviceName = deviceName;
        }
        else
        {
            Debug.LogError($"Device {deviceName} not found");
        }
    }
    
    /// <summary>
    /// Check if currently recording
    /// </summary>
    public bool IsRecording => _isRecording;
}