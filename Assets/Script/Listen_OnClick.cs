using UnityEngine;
using Oculus.Voice.Dictation;

public class Listen_OnClick : MonoBehaviour
{
    public AppDictationExperience appVoiceDictationExperience;
    public string transcription;
    public bool isTranscriptionGenerated;

    public void ActivateVoiceOnClick()
    {
        transcription = "";
        appVoiceDictationExperience.Activate();
        isTranscriptionGenerated = false;
    }
    public void PrintFinalTranscription(string s)
    {
        transcription += $"\n{s}";
        Debug.Log(transcription);
        isTranscriptionGenerated = true;
    }
}
