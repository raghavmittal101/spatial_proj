using UnityEngine;
using Oculus.Voice.Dictation;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class Listen_OnClick : MonoBehaviour
{
    public TMPro.TextMeshProUGUI speakButtonText;
    //public TMPro.TextMeshProUGUI transcriptionText;
    public TMPro.TMP_InputField transcriptionText;
    public UnityAction<string> PartialTranscriptionAction, FullTranscriptionAction;
    public Button speakButton;
    public SpeechRecognitionController whisperSpeechRecognitionController;

    private void Start()
    {
        PartialTranscriptionAction += UpdateTranscriptionText;
        FullTranscriptionAction += UpdateTranscriptionText;
        //FullTranscriptionAction += mainScript.ProcessUserInput;
        FullTranscriptionAction += PrintFinalTranscription;


        speakButton.onClick.AddListener(ActivateVoiceOnClick);

        // for whisper AI
        whisperSpeechRecognitionController.onStartRecording.AddListener(OnStartListening);
        whisperSpeechRecognitionController.onSendRecording.AddListener(OnSendRecording);
        whisperSpeechRecognitionController.onResponse.AddListener(FullTranscriptionAction);
        whisperSpeechRecognitionController.onError.AddListener(OnError);
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

    private void OnError(string a)
    {
        speakButtonText.text = a;
        OnStoppedListening();
    }

    private IEnumerator WaitForSec(float t)
    {
        WaitForSeconds waitForSeconds = new(t);
        yield return waitForSeconds;
    }
}
