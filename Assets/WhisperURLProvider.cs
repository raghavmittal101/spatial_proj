using UnityEngine;

public class WhisperURLProvider : MonoBehaviour
{
    [SerializeField] string whisperServerUrl;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if UNITY_EDITOR
        var camera = GameObject.Find("Main Camera");
#else
        var camera = GameObject.Find("CenterEyeAnchor");
#endif
        RunWhisper whisper = camera.GetComponent<RunWhisper>();
        whisper.serverUrl = whisperServerUrl;
    }

}
