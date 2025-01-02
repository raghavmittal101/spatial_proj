using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainScript : MonoBehaviour
{
    public StreamOBJImporter streamOBJImporter;
    //public enum InputMethod { DropdownInput, VoiceInput};
    //public InputMethod inputMethod;
    [SerializeField] GameObject eventSystemUnity;
    [SerializeField] GameObject eventSystemOculus;
    [SerializeField] GameObject cameraUnity;
    [SerializeField] GameObject cameraOculus;


    public void Awake()
    {
        //eventSystemUnity = GameObject.Find("EventSystem");
        //eventSystemOculus = GameObject.Find("PointableCanvasModule");
    }
    public void Start()
    {
        
#if UNITY_EDITOR
        eventSystemUnity.SetActive(true);
        eventSystemOculus.SetActive(false);
        cameraUnity.SetActive(true);
        cameraOculus.SetActive(false);

        Debug.Log("in editor");
#else
        eventSystemUnity.SetActive(false);
        eventSystemOculus.SetActive(true);
        cameraUnity.SetActive(false);
        cameraOculus.SetActive(true);
        Debug.Log("Not in Editor");
#endif

    }
    public void ProcessUserInput(string transcription)
    {
        if (transcription.ToLower().Contains("aircraft"))
        {
            Debug.Log("It is an aircraft");
            streamOBJImporter.objectName = "E 45 Aircraft_obj";
        }
        else if (transcription.ToLower().Contains("penguin"))
        {
            Debug.Log("It is a penguin");
            streamOBJImporter.objectName = "PenguinBaseMesh";
        }
        streamOBJImporter.DownloadExistingObject();
    }
}
