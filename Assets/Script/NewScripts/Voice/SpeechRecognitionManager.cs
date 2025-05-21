using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Manages speech recognition process by coordinating audio recording and transcription
/// </summary>
public class SpeechRecognitionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject audioServiceGameObject;
    [SerializeField] private GameObject transcriptionServiceGameObject;
    
    [Header("Configuration")]
    [SerializeField] private int recordingDuration = 30;
    [SerializeField] private int recordingFrequency = 16000;
    
    [Header("Events")]
    public UnityEvent<string> OnTranscriptionComplete;
    public UnityEvent<string> OnError;
    public UnityEvent OnRecordingStarted;
    public UnityEvent OnRecordingStopped;
    public UnityEvent OnProcessingStarted;
    
    private IAudioService _audioService;
    private ITranscriptionService _transcriptionService;
    private bool _isInitialized = false;
    
    private void Awake()
    {
        InitializeServices();
    }
    
    private void InitializeServices()
    {
        // Get or create audio service
        if (audioServiceGameObject != null)
        {
            _audioService = audioServiceGameObject.GetComponent<IAudioService>();
        }
        
        if (_audioService == null)
        {
            Debug.LogWarning("Audio service not found, creating default Unity microphone service");
            _audioService = gameObject.AddComponent<UnityMicrophoneService>();
        }
        
        // Get or create transcription service
        if (transcriptionServiceGameObject != null)
        {
            _transcriptionService = transcriptionServiceGameObject.GetComponent<ITranscriptionService>();
        }
        
        if (_transcriptionService == null)
        {
            Debug.LogWarning("Transcription service not found, creating default Whisper service");
            _transcriptionService = gameObject.AddComponent<WhisperTranscriptionService>();
        }
        
        // Set up event handlers
        _transcriptionService.TranscriptionCompleted += HandleTranscriptionComplete;
        _transcriptionService.TranscriptionError += HandleTranscriptionError;
        _transcriptionService.TranscriptionStarted += HandleTranscriptionStarted;
        
        _isInitialized = true;
        Debug.Log("Speech recognition manager initialized");
    }
    
    private void OnDestroy()
    {
        if (_transcriptionService != null)
        {
            _transcriptionService.TranscriptionCompleted -= HandleTranscriptionComplete;
            _transcriptionService.TranscriptionError -= HandleTranscriptionError;
            _transcriptionService.TranscriptionStarted -= HandleTranscriptionStarted;
        }
    }
    
    /// <summary>
    /// Configure the Whisper server URL for transcription
    /// </summary>
    public void ConfigureTranscriptionService(string serverUrl)
    {
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        _transcriptionService.Configure(serverUrl);
    }
    
    /// <summary>
    /// Get available audio devices
    /// </summary>
    public string[] GetAvailableAudioDevices()
    {
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        return _audioService.GetAvailableDevices();
    }
    
    /// <summary>
    /// Set the audio device to use for recording
    /// </summary>
    public void SetAudioDevice(string deviceName)
    {
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        _audioService.SetAudioDevice(deviceName);
    }
    
    /// <summary>
    /// Toggle recording state (start/stop)
    /// </summary>
    public void ToggleRecording()
    {
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        if (_audioService.IsRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }
    
    /// <summary>
    /// Start recording audio
    /// </summary>
    public void StartRecording()
    {
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        try
        {
            bool success = _audioService.StartRecording(recordingDuration, recordingFrequency);
            
            if (success)
            {
                OnRecordingStarted?.Invoke();
                Debug.Log("Recording started");
            }
            else
            {
                OnError?.Invoke("Failed to start recording");
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Error starting recording: {ex.Message}");
            Debug.LogError($"Error starting recording: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Stop recording and start transcription
    /// </summary>
    public void StopRecording()
    {
        if (!_isInitialized || !_audioService.IsRecording)
        {
            return;
        }
        
        try
        {
            AudioClip recordedClip = _audioService.StopRecording();
            OnRecordingStopped?.Invoke();
            
            if (recordedClip != null)
            {
                _transcriptionService.TranscribeAudio(recordedClip);
            }
            else
            {
                OnError?.Invoke("No audio recorded");
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Error stopping recording: {ex.Message}");
            Debug.LogError($"Error stopping recording: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get current recording progress (0-1)
    /// </summary>
    public float GetRecordingProgress()
    {
        if (!_isInitialized || !_audioService.IsRecording)
        {
            return 0f;
        }
        
        return _audioService.GetRecordingProgress();
    }
    
    /// <summary>
    /// Handle transcription complete event
    /// </summary>
    private void HandleTranscriptionComplete(string transcription)
    {
        OnTranscriptionComplete?.Invoke(transcription);
    }
    
    /// <summary>
    /// Handle transcription error event
    /// </summary>
    private void HandleTranscriptionError(string errorMessage)
    {
        OnError?.Invoke(errorMessage);
    }
    
    /// <summary>
    /// Handle transcription started event
    /// </summary>
    private void HandleTranscriptionStarted()
    {
        OnProcessingStarted?.Invoke();
    }
    
    /// <summary>
    /// Check if currently recording
    /// </summary>
    public bool IsRecording => _isInitialized && _audioService.IsRecording;
    
    /// <summary>
    /// Check if currently transcribing
    /// </summary>
    public bool IsTranscribing => _isInitialized && _transcriptionService.IsTranscribing;
}