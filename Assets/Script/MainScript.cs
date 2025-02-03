using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainScript : MonoBehaviour
{
    public StreamOBJImporter streamOBJImporter;
    public ImageFetcher imageFetcher;
    //public enum InputMethod { DropdownInput, VoiceInput};
    //public InputMethod inputMethod;
    [SerializeField] GameObject eventSystemUnity;
    [SerializeField] GameObject eventSystemOculus;
    [SerializeField] GameObject cameraUnity;
    [SerializeField] GameObject cameraOculus;
    //[SerializeField] Dictionary<string, string> inputObjectNamePairs;
    //= { "aircraft", "penguin", "axe", "tralismesh" };
    //= { "E 45 Aircraft_obj", "PenguinBaseMesh", "Axe", "07_01_2025_16_29_33_1" };
    [SerializeField] public List<string> valueInDropdown;
    [SerializeField] public List<string> objNameOnServer;
    [SerializeField] public TMPro.TMP_Dropdown objectsDropdown;

    public void Awake()
    {
        //eventSystemUnity = GameObject.Find("EventSystem");
        //eventSystemOculus = GameObject.Find("PointableCanvasModule");
    }
    public void Start()
    {
        objectsDropdown.AddOptions(valueInDropdown);

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
        foreach (var i in valueInDropdown)
        {
            if(transcription == i)
            {
                Debug.Log($"The object name on the server should be \"{objNameOnServer[valueInDropdown.IndexOf(i)]}\"");
                streamOBJImporter.objectName = objNameOnServer[valueInDropdown.IndexOf(i)];
            }
        }
        streamOBJImporter.DownloadExistingObject();
    }

    public void ProcessUserPrompt(string prompt)
    {
        streamOBJImporter.objectName = prompt;
        streamOBJImporter.GetObjMesh();
    }
    public void ProcessUserPromptForImage(string prompt)
    {
        streamOBJImporter.objectName = prompt;
        streamOBJImporter.GetObjImage();
    }
}