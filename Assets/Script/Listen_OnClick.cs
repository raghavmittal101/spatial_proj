using UnityEngine;
using Oculus.Voice.Dictation;
using UnityEngine.Events;
using UnityEngine.UI;

public class Listen_OnClick : MonoBehaviour
{
    public AppDictationExperience appVoiceDictationExperience;
    public TMPro.TextMeshProUGUI speakButtonText;
    //public TMPro.TextMeshProUGUI transcriptionText;
    public TMPro.TMP_InputField transcriptionText;
    public MainScript mainScript;
    public UnityAction<string> PartialTranscriptionAction, FullTranscriptionAction;
    public Button speakButton;
    public SpeechRecognitionController whisperSpeechRecognitionController;

    private void Start()
    {
        PartialTranscriptionAction += UpdateTranscriptionText;
        FullTranscriptionAction += UpdateTranscriptionText;
        //FullTranscriptionAction += mainScript.ProcessUserInput;
        FullTranscriptionAction += PrintFinalTranscription;

        appVoiceDictationExperience.DictationEvents.OnStartListening.AddListener(OnStartListening);
        appVoiceDictationExperience.DictationEvents.OnStoppedListening.AddListener(OnStoppedListening);
        
        appVoiceDictationExperience.DictationEvents.OnPartialTranscription.AddListener(PartialTranscriptionAction);
        appVoiceDictationExperience.DictationEvents.OnFullTranscription.AddListener(FullTranscriptionAction);

        speakButton.onClick.AddListener(ActivateVoiceOnClick);

        // for whisper AI
        whisperSpeechRecognitionController.onStartRecording.AddListener(OnStartListening);
        whisperSpeechRecognitionController.onSendRecording.AddListener(OnSendRecording);
        whisperSpeechRecognitionController.onResponse.AddListener(FullTranscriptionAction);
    }
    private void OnStartListening()
    {
        speakButtonText.text = "Press when you are done";
    }
    private void OnStoppedListening()
    {
        speakButtonText.text = "Press to speak!";
        speakButton.interactable = true;
    }

    private void OnSendRecording()
    {
        speakButtonText.text = "Processing...";
        speakButton.interactable = false;
    }

    public void ActivateVoiceOnClick()
    {
        Debug.Log("Clicked on button");
        transcriptionText.text = "";
        appVoiceDictationExperience.Activate();
    }

    
    public void PrintFinalTranscription(string s)
    {
        Debug.Log(s);
        OnStoppedListening();

    }

    private void UpdateTranscriptionText(string a)
    {
        transcriptionText.SetTextWithoutNotify(a);
        OnStoppedListening();
    }
}
