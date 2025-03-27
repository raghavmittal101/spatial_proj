using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Dummiesman;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StreamOBJImporter : MonoBehaviour
{
    public string objectName;
    [SerializeField] private string predefinedObjHostURL = "http://100.64.0.1:5000/";
    [SerializeField] private string text23DHostURL = null;
    [SerializeField] private string apiVersion = "v0";

    [SerializeField] private TextMeshProUGUI statusTextVariable;
    [SerializeField] private GameObject errorTextPrefab;
    [SerializeField] private GameObject errorScrollbarContainer;
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject mainPanel;

    private GameObject objPrefabPlaceholder;
    [SerializeField] private GameObject grabbableObjContainerPrefab;
    [SerializeField] private GameObject objPrefabPlaceholderAR;
    [SerializeField] private GameObject objPrefabPlaceholderVR;
    [SerializeField] private _ToggleMaker VR2ARToggleButton;
    [SerializeField] private GameObject envBoundingCube;
    [SerializeField] private MainScript mainScript;
    [SerializeField] private ImageFetcher imageFetcher;

    [Header("For robot integration")]
    public bool GenerateForRobo;
    public bool IsAPlatform { get; set; }
    public bool PlaceOutOfRoboRange { get; set; }
    [SerializeField] private TrajectoryPlanner trajectoryPlanner;
    [SerializeField] private GameObject grabbableObjContainerPrefabForRobot;
    [SerializeField] private GameObject outOfRangeObjectPlaceholder_roboscene;
    [SerializeField] private GameObject loadedObjectPlaceholder_roboscene;
    [SerializeField] private GameObject platformObjectPlacehodler_roboscene;

    private string objFileFetchURL;
    private string objZipFetchURL;
    private string texURL;
    private string mtlURL;
    private string objURL;
    private string zipPath;
    private string downloadedMeshDirPath;
    private string text23DGenURL;
    private string image23DGenURL;
    private string text2ImageGenURL;

    [System.Serializable]
    public class ComponentURLs
    {
        public string obj_file;
        public string mtl_file;
        public string tex_file;
    }

    //private ComponentURLs componentURLs;
    //private Stream[] objComponentsMemStreams;

    void Start()
    {
        objFileFetchURL = predefinedObjHostURL + apiVersion + "/download-asset/"; // Replace with your API endpoint
        objZipFetchURL = predefinedObjHostURL + apiVersion + "/download-asset-compressed/";
        image23DGenURL = text23DHostURL + apiVersion + "/img-2-3d/";
        text2ImageGenURL = text23DHostURL + apiVersion + "/text-2-img/";
        //objComponentsMemStreams = new Stream[3];
        text23DGenURL = text23DHostURL + apiVersion + "/text-2-3d/";
        //DownloadExistingObject();
        VR2ARToggleButton.onValueChanged.AddListener(_OnValueChanged);
        objPrefabPlaceholder = objPrefabPlaceholderVR;

    }

    private void ResetSceneOnFail()
    {
        statusTextVariable.text = "Load";
    }

    public void PrintErrorToScreen(string message)
    {
        var errorTextObj = Instantiate(errorTextPrefab, errorScrollbarContainer.transform);
        errorTextObj.GetComponent<TextMeshProUGUI>().text = $"-> {message}";
    }

    private IEnumerator RenderDownloadedMesh(string objName, InputMode inputMode)
    {


        try
        {
            statusPanel?.SetActive(true);
            mainPanel?.SetActive(false);
            yield return StartCoroutine(DownloadAndExtractZip(objName, inputMode));
        }
        finally
        { }




        try
        {
            if (inputMode != InputMode.textToImage)
            {
                downloadedMeshDirPath = Path.Combine(Application.persistentDataPath, zipURL.asset_name);
                string objfilePath = downloadedMeshDirPath + "/" + zipURL.asset_name + ".obj";
                string mtlfilePath = downloadedMeshDirPath + "/" + zipURL.asset_name + ".mtl";
                Debug.Log(objfilePath);
                statusTextVariable.text = "Processing the object for rendering...";
                var loadedObj = new OBJLoader().Load(objfilePath, mtlfilePath);
                GameObject grabbableObj;
                if (GenerateForRobo)
                {
                    if (IsAPlatform)
                    {
                        // generate for platform. everything else is ignored
                        Debug.Log("generate for platform. everything else is ignored");
                        grabbableObj = Instantiate(grabbableObjContainerPrefab);
                        objPrefabPlaceholder = platformObjectPlacehodler_roboscene;
                    }
                    else
                    {
                        if (PlaceOutOfRoboRange)
                        {
                            // it is not a platform and use wants to place the object out of range.
                            Debug.Log("it is not a platform and use wants to place the object out of range.");
                            grabbableObj = Instantiate(grabbableObjContainerPrefabForRobot);
                            objPrefabPlaceholder = outOfRangeObjectPlaceholder_roboscene;

                        }
                        else
                        {
                            // it is not for a platform, and the user wants to place objects within the range.
                            Debug.Log("it is not a platform, and the user wants to place objects within the range.");
                            grabbableObj = Instantiate(grabbableObjContainerPrefabForRobot);
                            objPrefabPlaceholder = loadedObjectPlaceholder_roboscene;
                        }
                    }
                    grabbableObj.transform.position = new Vector3(
                        objPrefabPlaceholder.transform.position.x,
                        objPrefabPlaceholder.transform.position.y + 0.05f,
                        objPrefabPlaceholder.transform.position.z);
                    grabbableObj.transform.localScale = new Vector3(
                        1f,
                        1f,
                        1f);
                }
                else
                {
                    grabbableObj = Instantiate(grabbableObjContainerPrefab);
                    grabbableObj.transform.position = new Vector3(
                        objPrefabPlaceholder.transform.position.x,
                        objPrefabPlaceholder.transform.position.y + 0.05f,
                        objPrefabPlaceholder.transform.position.z);
                    grabbableObj.transform.localScale = new Vector3(
                        1.5f,
                        1.5f,
                        1.5f);
                }

                //    if(!isAPlatform)
                        
                //    else if (placeOutOfRoboRange)
                //    {
                //        grabbableObj = Instantiate(grabbableObjContainerPrefabForRobot);
                //        objPrefabPlaceholder = outOfRangeObjectPlaceholder_roboscene;

                //    }
                        
                //    else
                //        grabbableObj = Instantiate(grabbableObjContainerPrefab);
                //}
                //else
                //{
                //    grabbableObj = Instantiate(grabbableObjContainerPrefab, objPrefabPlaceholder.transform);
                //}

                
                if (GenerateForRobo && !IsAPlatform)
                {
                    GameObject boundingcube = grabbableObj.transform.Find("BoundingCube").gameObject;
                    
                    if (boundingcube != null)
                    {
                        var addToPubQueue = boundingcube.GetComponent<AddToPublishQueue>();
                        addToPubQueue.trajectoryPlanner = trajectoryPlanner;
                    }
                }
                
                Transform grabbableObjBoundingCubeTransform = grabbableObj.transform.GetChild(0).transform;
                Bounds boundsOfLoadedObj = _GetChildRendererBounds(loadedObj);
                //grabbableObjBoundingCubeTransform.transform.localScale = (boundsOfLoadedObj.size);
                grabbableObjBoundingCubeTransform.GetComponent<BoxCollider>().size = boundsOfLoadedObj.size;
                loadedObj.transform.parent = grabbableObjBoundingCubeTransform.transform;
                loadedObj.transform.localPosition = Vector3.zero;
                loadedObj.transform.localScale = new Vector3(1, 1, 1);
                if (inputMode == InputMode.prompt)
                {
                    mainScript.valueInDropdown.Add(objName);
                    mainScript.objNameOnServer.Add(zipURL.asset_name);
                    mainScript.objectsDropdown.enabled = false;
                    mainScript.objectsDropdown.ClearOptions();
                    mainScript.objectsDropdown.AddOptions(mainScript.valueInDropdown);
                    mainScript.objectsDropdown.enabled = true;
                }
            }

        }
        catch (System.Exception e)
        {
            if (inputMode != InputMode.textToImage)
            {
                if (inputMode != InputMode.textToImage)
                {
                    Debug.Log("There is some issue was rendering the object.");
                    Debug.LogError(e);
                    PrintErrorToScreen("There was some issue in rendering the object.");
                }
            }
        }
        finally
        {
            try
            {
                if (inputMode != InputMode.textToImage)
                    Directory.Delete(downloadedMeshDirPath, true);
            }
            catch
            {
                if (inputMode != InputMode.textToImage)
                {
                    Debug.LogError("Unable to delete extracted mesh directory");
                    PrintErrorToScreen("Unable to delete extracted mesh directory");
                }
            }
            finally
            {
                try { statusPanel?.SetActive(false); mainPanel?.SetActive(true); }
                catch { }
                statusTextVariable.text = "Load";
            }

        }


    }

    private Bounds _GetChildRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var rend in renderers)
            {
                bounds.Encapsulate(rend.bounds);
            }
            return bounds;
        }
        else { return new Bounds(); }
    }

    [System.Serializable]
    public class ZipURL
    {
        public string zip_file;
        public string asset_name;
    }

    public class Text2ImageResponseBody
    {
        public string img_file;
        public string asset_name;
    }

    private ZipURL zipURL;
    private Text2ImageResponseBody text2ImageResponseBody;

    private enum InputMode
    {
        prompt,
        objectName,
        imageAssetName,
        textToImage
    }

    // Downloads zip file from server to persistent path and returns the folder path
    private IEnumerator DownloadAndExtractZip(string objName, InputMode inputMode)
    {
        WWWForm form = new WWWForm();
        string objectFetchURL = "";
        string hostURL = "";

        if (inputMode == InputMode.objectName)
        {
            form.AddField("asset_name", objName);
            zipURL.asset_name = objName;
            zipPath = Path.Combine(Application.persistentDataPath, zipURL.asset_name + ".zip");
            statusTextVariable.text = "Fetching the object...";
        }
        else if (inputMode == InputMode.prompt)
        {
            form.AddField("prompt", objName);
            objectFetchURL = text23DGenURL;
            hostURL = text23DHostURL;
            statusTextVariable.text = "Generating the object. This may take some time...";

            yield return Fetcher(hostURL, objectFetchURL, form);

        }

        else if (inputMode == InputMode.imageAssetName)
        {
            form.AddField("assetname", objName);
            objectFetchURL = image23DGenURL;
            hostURL = text23DHostURL;
            statusTextVariable.text = "Generating the object. This may take some time...";

            yield return Fetcher(hostURL, objectFetchURL, form);
        }
        else if (inputMode == InputMode.textToImage)
        {
            Debug.Log("reached to 291 line");
            form.AddField("prompt", objName);
            objectFetchURL = text2ImageGenURL;
            hostURL = text23DHostURL;
            statusTextVariable.text = "Generating the object image. This may take some time...";

            yield return FetchImage(objectFetchURL, form, objName);
        }


        try
        {
            if (inputMode != InputMode.textToImage)
            {
                // extract file
                string extractPath = Application.persistentDataPath;
                statusTextVariable.text = "Extracting the compressed object...";
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
        }
        catch
        {
            if (inputMode != InputMode.textToImage)
            {
                Debug.LogError("failed to extract downloaded zip file.");
                PrintErrorToScreen("failed to extract downloaded object zip file.");
                File.Delete(zipPath);
                ResetSceneOnFail();
                yield break;
            }
        }


    }


    IEnumerator FetchImage(string imageFetchURL, WWWForm form, string objName)
    {
        using (var w = UnityWebRequest.Post(imageFetchURL, form))
        {
            Debug.Log("reached to 322 line");
            Debug.Log("imageFetchURL: " + imageFetchURL);
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(w.error);
                PrintErrorToScreen("Something went wrong while trying to get image file URL.");
                ResetSceneOnFail();
                yield break;
            }
            else
            {
                Debug.Log("finished");
                text2ImageResponseBody = JsonUtility.FromJson<Text2ImageResponseBody>(w.downloadHandler.text);
                Debug.Log("text2ImageResponseBody.image_file: " + text2ImageResponseBody.img_file);

                StartCoroutine(imageFetcher.DownloadImageAndSetTheButton(text23DHostURL + text2ImageResponseBody.img_file, text2ImageResponseBody.asset_name, objName));

            }
        }

    }

    IEnumerator Fetcher(string hostURL, string objectFetchURL, WWWForm form)
    {
        using (var w = UnityWebRequest.Post(objectFetchURL, form))
        {
            Debug.Log(objectFetchURL);
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(w.error);
                PrintErrorToScreen("Something went wrong while trying to get the object zipfile URL.");
                ResetSceneOnFail();
                yield break;
            }
            else
            {
                Debug.Log("finished");
                zipURL = JsonUtility.FromJson<ZipURL>(w.downloadHandler.text);

                // now download the file
                zipPath = Path.Combine(Application.persistentDataPath, zipURL.asset_name + ".zip");
                statusTextVariable.text = "Downloading the generated object...";
                using (UnityWebRequest www = UnityWebRequest.Get(hostURL + zipURL.zip_file))
                {

                    yield return www.SendWebRequest();
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                        PrintErrorToScreen("Something went wrong while trying to download object zip file.");
                        ResetSceneOnFail();
                        yield return null;
                    }
                    else
                    {
                        byte[] result = www.downloadHandler.data;
                        Debug.Log("Download size:" + result.Length / (1024 * 1024) + " MB");
                        using (FileStream SourceStream = File.Open(zipPath, FileMode.OpenOrCreate))
                        {
                            if (!SourceStream.CanWrite)
                            {
                                Debug.LogError("Unable to write to the downloads directory");
                                PrintErrorToScreen("Unable to write to the downloads directory");
                                ResetSceneOnFail();
                                yield break;
                            }
                            else
                            {
                                SourceStream.Seek(0, SeekOrigin.End);
                                Task task = SourceStream.WriteAsync(result, 0, result.Length);
                                yield return new WaitUntil(() => task.IsCompleted);
                            }
                        }
                    }

                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            Directory.Delete(downloadedMeshDirPath, true);
            File.Delete(zipPath);
        }
        catch { }

    }

    [System.Serializable]
    public class ConvertedFileURL
    {
        public string converted_compressed_file_url;
    }
    private ConvertedFileURL convertedFileURL;

    //public IEnumerator UploadFileAndConvert(string filename, string filePath){
    //    // List formData = new List();
    //    byte[] filedata = File.ReadAllBytes(filePath);
    //    // files[0] = files[0].Replace(@“.",”");
    //    // formData.Add(new MultipartFormFileSection(“file”, bytes, files[0], “text/plain”));
    //    // StartCoroutine(UploadFile(formData));
    //    var convertTo3dUrl= hostURL + apiVersion + "/convert-to-3d/";
    //    WWWForm form = new WWWForm();
    //    form.AddBinaryData("file", filedata, filename);
    //    using (var w = UnityWebRequest.Post(convertTo3dUrl, form))
    //    {
    //        yield return w.SendWebRequest();
    //        if (w.result != UnityWebRequest.Result.Success) {
    //            Debug.Log(w.error);
    //        }
    //        else {
    //            Debug.Log("finished");
    //            convertedFileURL = JsonUtility.FromJson<ConvertedFileURL>(w.downloadHandler.text);
    //        }
    //    }

    //}

    //public void UploadFileAndConvert(){
    //    StartCoroutine(UploadFileAndConvert("abc.png", "/home/raghav/Downloads/spatialsuiteSystem1.drawio (2).png"));
    //}

    public void DownloadExistingObject()
    {
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.objectName));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for (int i = 0; i < errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }

    public void GetObjMesh()
    {
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.prompt));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for (int i = 0; i < errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }

    public void GetObjImage()
    {
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.textToImage));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for (int i = 0; i < errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }

    public void GetMeshFromImgAssetName(string objectName)
    {
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.imageAssetName));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for (int i = 0; i < errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }

    public void EnableARMode()
    {
        _OnValueChanged(true);
    }
    private void _OnValueChanged(bool enabled)
    {
        if (enabled)
        {
            objPrefabPlaceholder = objPrefabPlaceholderAR;
            envBoundingCube.SetActive(false);
            objPrefabPlaceholderAR.SetActive(true);

        }
        else
        {
            objPrefabPlaceholder = objPrefabPlaceholderVR;
            envBoundingCube.SetActive(true);
            objPrefabPlaceholderAR.SetActive(false);
        }
    }
}