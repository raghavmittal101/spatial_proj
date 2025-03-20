using UnityEngine;

public class InitRoboScene : MonoBehaviour
{
    [SerializeField]
    FullScaleModeSwitcher fullScaleModeSwitcher;
    [SerializeField]
    StreamOBJImporter streamOBJImporter;
    [SerializeField]
    GameObject roboSetup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fullScaleModeSwitcher.EnableFullScaleMode();
        streamOBJImporter.EnableARMode();
        roboSetup.SetActive(true);
    }
}
