using UnityEngine;
using Oculus.Voice.Dictation;
using UnityEngine.Events;

public class Listen_OnClick : MonoBehaviour
{
    public AppDictationExperience appVoiceDictationExperience;
    public TMPro.TextMeshProUGUI speakButtonText;
    public TMPro.TextMeshProUGUI transcriptionText;
    public MainScript mainScript;
    public UnityAction<string> PartialTranscriptionAction, FullTranscriptionAction;

    private void Start()
    {
        PartialTranscriptionAction += UpdateTranscriptionText;
        FullTranscriptionAction += UpdateTranscriptionText;
        FullTranscriptionAction += mainScript.ProcessUserInput;
        FullTranscriptionAction += PrintFinalTranscription;

        appVoiceDictationExperience.DictationEvents.OnStartListening.AddListener(OnStartListening);
        appVoiceDictationExperience.DictationEvents.OnStoppedListening.AddListener(OnStoppedListening);
        
        appVoiceDictationExperience.DictationEvents.OnPartialTranscription.AddListener(PartialTranscriptionAction);
        appVoiceDictationExperience.DictationEvents.OnFullTranscription.AddListener(FullTranscriptionAction);
    }
    private void OnStartListening()
    {
        speakButtonText.text = "Listening...";
    }
    private void OnStoppedListening()
    {
        speakButtonText.text = "Press to speak!";
    }

    public void ActivateVoiceOnClick()
    {
        transcriptionText.text = "";
        appVoiceDictationExperience.Activate();
    }

    
    public void PrintFinalTranscription(string s)
    {
        Debug.Log(s);
    }

    private void UpdateTranscriptionText(string a)
    {
        transcriptionText.text = a;
    }
}
