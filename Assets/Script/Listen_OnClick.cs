using UnityEngine;
using Oculus.Voice.Dictation;

public class Listen_OnClick : MonoBehaviour
{
    public AppDictationExperience appVoiceDictationExperience;
    public string transcription;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateVoiceOnClick()
    {
        transcription = "";
        appVoiceDictationExperience.Activate();
    }
    public void PrintFinalTranscription(string s)
    {
        transcription += $"\n{s}";
        Debug.Log(transcription);
    }
}
