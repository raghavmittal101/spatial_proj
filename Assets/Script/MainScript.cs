using UnityEngine;

public class MainScript : MonoBehaviour
{
    public StreamOBJImporter streamOBJImporter;
    public Listen_OnClick listen_OnClick;


    public void ProcessFinalTranscription()
    {
        if (listen_OnClick.transcription.ToLower().Contains("aircraft"))
        {
            Debug.Log("It is an aircraft");
            streamOBJImporter.objectName = "E 45 Aircraft_obj";
        }
        else if (listen_OnClick.transcription.ToLower().Contains("penguin"))
        {
            Debug.Log("It is a penguin");
            streamOBJImporter.objectName = "PenguinBaseMesh";
        }
        streamOBJImporter.DownloadExistingObject();
    }
}
