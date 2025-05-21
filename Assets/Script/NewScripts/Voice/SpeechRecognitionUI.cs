using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI controller for speech recognition system
/// </summary>
public class SpeechRecognitionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button recordButton;
    [SerializeField] private TextMeshProUGUI recordButtonText;
    [SerializeField] private TMP_InputField transcriptionField;
    [SerializeField] private Image progressIndicator;
    [SerializeField] private TMP_Dropdown deviceDropdown;
    
    [Header("Button Text")]
    [SerializeField] private string startRecordingText = "Press to speak";
    [SerializeField] private string stopRecordingText = "Press when done";
    [SerializeField] private string processingText = "Processing...";
    
    [Header("Components")]
    [SerializeField] private SpeechRecognitionManager speechManager;
    
    private void Awake()
    {
        // Ensure we have reference to speech recognition manager
        if (speechManager == null)
        {
            speechManager = GetComponent<SpeechRecognitionManager>();
            
            if (speechManager == null)
            {
                Debug.LogError("SpeechRecognitionManager reference is required");
                enabled = false;
                return;
            }
        }
    }
    
    private void Start()
    {
        // Set up button
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(OnRecordButtonClicked);
        }
        
        // Set up device dropdown
        if (deviceDropdown != null)
        {
            PopulateDeviceDropdown();
            deviceDropdown.onValueChanged.AddListener(OnDeviceDropdownChanged);
        }
        
        // Setup event handlers
        speechManager.OnRecordingStarted.AddListener(HandleRecordingStarted);
        speechManager.OnRecordingStopped.AddListener(HandleRecordingStopped);
        speechManager.OnProcessingStarted.AddListener(HandleProcessingStarted);
        speechManager.OnTranscriptionComplete.AddListener(HandleTranscriptionComplete);
        speechManager.OnError.AddListener(HandleError);
        
        // Initial UI state
        UpdateButtonText(startRecordingText);
        ClearTranscription();
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (recordButton != null)
        {
            recordButton.onClick.RemoveListener(OnRecordButtonClicked);
        }
        
        if (deviceDropdown != null)
        {
            deviceDropdown.onValueChanged.RemoveListener(OnDeviceDropdownChanged);
        }
        
        if (speechManager != null)
        {
            speechManager.OnRecordingStarted.RemoveListener(HandleRecordingStarted);
            speechManager.OnRecordingStopped.RemoveListener(HandleRecordingStopped);
            speechManager.OnProcessingStarted.RemoveListener(HandleProcessingStarted);
            speechManager.OnTranscriptionComplete.RemoveListener(HandleTranscriptionComplete);
            speechManager.OnError.RemoveListener(HandleError);
        }
    }
    
    private void Update()
    {
        // Update progress indicator if recording
        if (progressIndicator != null && speechManager.IsRecording)
        {
            progressIndicator.fillAmount = speechManager.GetRecordingProgress();
        }
    }
    
    /// <summary>
    /// Populate the device dropdown with available microphones
    /// </summary>
    private void PopulateDeviceDropdown()
    {
        if (deviceDropdown == null)
        {
            return;
        }
        
        deviceDropdown.ClearOptions();
        
        string[] devices = speechManager.GetAvailableAudioDevices();
        if (devices.Length > 0)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string device in devices)
            {
                options.Add(new TMP_Dropdown.OptionData(device));
            }
            
            deviceDropdown.AddOptions(options);
            deviceDropdown.value = 0;
        }
    }
    
    /// <summary>
    /// Handle record button click
    /// </summary>
    public void OnRecordButtonClicked()
    {
        if (speechManager.IsTranscribing)
        {
            return; // Ignore clicks while processing
        }
        
        if (!speechManager.IsRecording)
        {
            ClearTranscription();
        }
        
        speechManager.ToggleRecording();
    }
    
    /// <summary>
    /// Handle device dropdown change
    /// </summary>
    private void OnDeviceDropdownChanged(int index)
    {
        string[] devices = speechManager.GetAvailableAudioDevices();
        if (index >= 0 && index < devices.Length)
        {
            speechManager.SetAudioDevice(devices[index]);
        }
    }
    
    /// <summary>
    /// Update the record button text
    /// </summary>
    private void UpdateButtonText(string text)
    {
        if (recordButtonText != null)
        {
            recordButtonText.text = text;
        }
    }
    
    /// <summary>
    /// Clear the transcription field
    /// </summary>
    private void ClearTranscription()
    {
        if (transcriptionField != null)
        {
            transcriptionField.SetTextWithoutNotify("");
        }
    }
    
    /// <summary>
    /// Update the transcription field with transcribed text
    /// </summary>
    private void UpdateTranscription(string text)
    {
        if (transcriptionField != null)
        {
            transcriptionField.SetTextWithoutNotify(text);
        }
    }
    
    /// <summary>
    /// Set interactability of UI elements
    /// </summary>
    private void SetUIInteractable(bool interactable)
    {
        if (recordButton != null)
        {
            recordButton.interactable = interactable;
        }
        
        if (deviceDropdown != null)
        {
            deviceDropdown.interactable = interactable;
        }
    }
    
    #region Event Handlers
    
    private void HandleRecordingStarted()
    {
        UpdateButtonText(stopRecordingText);
        if (deviceDropdown != null)
        {
            deviceDropdown.interactable = false;
        }
    }
    
    private void HandleRecordingStopped()
    {
        if (progressIndicator != null)
        {
            progressIndicator.fillAmount = 0f;
        }
    }
    
    private void HandleProcessingStarted()
    {
        UpdateButtonText(processingText);
        SetUIInteractable(false);
    }
    
    private void HandleTranscriptionComplete(string transcription)
    {
        UpdateTranscription(transcription);
        UpdateButtonText(startRecordingText);
        SetUIInteractable(true);
    }
    
    private void HandleError(string errorMessage)
    {
        Debug.LogError($"Speech Recognition Error: {errorMessage}");
        UpdateButtonText(errorMessage);
        SetUIInteractable(true);
        
        // Reset UI after a delay
        Invoke(nameof(ResetUI), 3f);
    }
    
    private void ResetUI()
    {
        UpdateButtonText(startRecordingText);
    }
    
    #endregion
}