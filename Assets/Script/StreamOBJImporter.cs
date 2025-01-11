using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Dummiesman;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class StreamOBJImporter : MonoBehaviour {
    public string objectName;
    [SerializeField] private string predefinedObjHostURL = "http://100.64.0.1:5000/";
    [SerializeField] private string text23DHostURL = null;
    [SerializeField] private string apiVersion = "v0";
    
    [SerializeField] private TextMeshProUGUI statusTextVariable;
    [SerializeField] private GameObject errorTextPrefab;
    [SerializeField] private GameObject errorScrollbarContainer;
    [SerializeField] private GameObject statusPanel;

    [SerializeField] private GameObject grabbableObjContainerPrefab;
    [SerializeField] private GameObject objPrefabPlaceholder;

    private string objFileFetchURL;
    private string objZipFetchURL;
    private string texURL;
    private string mtlURL;
    private string objURL;
    private string zipPath;
    private string downloadedMeshDirPath;
    private string text23DGenURL;
    //private MemoryStream obj_memStream;
    //private MemoryStream mtl_memStream;
    //private MemoryStream tex_memStream;

    [System.Serializable]
    public class ComponentURLs
    {
        public string obj_file;
        public string mtl_file;
        public string tex_file;
    }

    //private ComponentURLs componentURLs;
    //private Stream[] objComponentsMemStreams;

	void Start () { 
        objFileFetchURL = predefinedObjHostURL + apiVersion + "/download-asset/"; // Replace with your API endpoint
        objZipFetchURL =  predefinedObjHostURL + apiVersion + "/download-asset-compressed/";
        //objComponentsMemStreams = new Stream[3];
        text23DGenURL = text23DHostURL + apiVersion + "/text-2-3d/";
        //DownloadExistingObject();

    }

    private void ResetSceneOnFail()
    {
        statusTextVariable.text = "Load";
    }

    private void PrintErrorToScreen(string message)
    {
        var errorTextObj = Instantiate(errorTextPrefab, errorScrollbarContainer.transform);
        errorTextObj.GetComponent<TextMeshProUGUI>().text = $"-> {message}";
    }

    private IEnumerator RenderDownloadedMesh(string objName, InputMode inputMode){
        
        
        try
        {
            statusPanel?.SetActive(true);
            yield return StartCoroutine(DownloadAndExtractZip(objName, inputMode));
        }
        finally
        { }
        
        try
        {
            downloadedMeshDirPath = Path.Combine(Application.persistentDataPath, zipURL.asset_name);
            string objfilePath = downloadedMeshDirPath + "/" + zipURL.asset_name + ".obj";
            string mtlfilePath = downloadedMeshDirPath + "/" + zipURL.asset_name + ".mtl";
            Debug.Log(objfilePath);
            statusTextVariable.text = "Processing the object for rendering...";
            var loadedObj = new OBJLoader().Load(objfilePath, mtlfilePath);
            var grabbableObj = Instantiate(grabbableObjContainerPrefab, objPrefabPlaceholder.transform);
            grabbableObj.transform.position = new Vector3(
                objPrefabPlaceholder.transform.position.x,
                objPrefabPlaceholder.transform.position.y+0.1f,
                objPrefabPlaceholder.transform.position.z);
            Transform grabbableObjBoundingCubeTransform = grabbableObj.transform.GetChild(0).transform;
            Bounds boundsOfLoadedObj = _GetChildRendererBounds(loadedObj);
            //grabbableObjBoundingCubeTransform.transform.localScale = (boundsOfLoadedObj.size);
            grabbableObjBoundingCubeTransform.GetComponent<BoxCollider>().size = boundsOfLoadedObj.size;
            loadedObj.transform.parent = grabbableObjBoundingCubeTransform.transform;
            loadedObj.transform.localPosition = Vector3.zero;
            loadedObj.transform.localScale = new Vector3(1, 1, 1);
            
        }
        catch(System.Exception e)
        {
            Debug.Log("There is some issue was rendering the object.");
            Debug.LogError(e);
            PrintErrorToScreen("There was some issue in rendering the object.");
        }
        finally
        {
            
            //currentBc.center = new Vector3(0f, (boundsOfLoadedObj.size.y / 2), 0f);
            
            try
            {
                Directory.Delete(downloadedMeshDirPath, true);
            }
            catch
            {
                Debug.LogError("Unable to delete extracted mesh directory");
                PrintErrorToScreen("Unable to delete extracted mesh directory");
            }
            try
            {
                File.Delete(zipPath);
            }
            catch
            {
                Debug.LogError("Unable to delete downloaded zipfile");
                PrintErrorToScreen("Unable to delete downloaded zipfile");
            }
            finally
            {
                try { statusPanel?.SetActive(false); }
                catch { }
                statusTextVariable.text = "Load";
            }
            
        }
        
        //Rigidbody currentRb  = loadedObj.AddComponent<Rigidbody>();
        //currentRb.useGravity = false;
        //currentRb.isKinematic = true;
        //currentRb.automaticCenterOfMass = true;
        //currentRb.mass = 1;
        //currentRb.angularDamping = 0.5f;
        

    }

    //private IEnumerator RenderStreamedMesh(string objName){
    //    yield return StartCoroutine(GetObjComponentsURLs(objName));
    //    yield return StartCoroutine(DownloadFile(hostURL + componentURLs.mtl_file, 0)); // mtl_file
    //    yield return StartCoroutine(DownloadFile(hostURL + componentURLs.tex_file, 1)); // tex_file
    //    yield return StartCoroutine(DownloadFile(hostURL + componentURLs.obj_file, 2)); // obj_file
    //    Debug.Log("now running ObjLoader......");
    //    var loadedObj = new OBJLoader().Load(objComponentsMemStreams[2], objComponentsMemStreams[0]);
    //}

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

    //private IEnumerator GetObjComponentsURLs(string objName)
    //{
    //    WWWForm form = new WWWForm();
    //    form.AddField("asset_name", objName);
    //    using (var w = UnityWebRequest.Post(objFileFetchURL, form))
    //    {
    //        yield return w.SendWebRequest();
    //        if (w.result != UnityWebRequest.Result.Success) {
    //            Debug.Log(w.error);
    //        }
    //        else {
    //            Debug.Log("finished");
    //            componentURLs = JsonUtility.FromJson<ComponentURLs>(w.downloadHandler.text);
    //        }
    //    }
    //}

    //private IEnumerator DownloadFile(string fileURL, int streamsArrayIndex){
    //    Debug.Log("Downloading a file...");
    //    MemoryStream textStream;
    //    using(var www = new WWW(fileURL)){
    //        yield return www;
    //        textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.text));
    //        Debug.Log("downloading for "+streamsArrayIndex+" done! ");
    //        objComponentsMemStreams[streamsArrayIndex] = textStream;
    //        Debug.Log(Encoding.UTF8.GetByteCount(www.text));
    //    }
        
    //}


    [System.Serializable]
    public class ZipURL
    {
        public string zip_file;
        public string asset_name;
    }

    private ZipURL zipURL;
    private enum InputMode
    {
        prompt,
        objectName
    }

    // Downloads zip file from server to persistent path and returns the folder path
    private IEnumerator DownloadAndExtractZip(string objName, InputMode inputMode){
        statusTextVariable.text = "Fetching the object URL...";
        WWWForm form = new WWWForm();
        string objectFetchURL = "";
        string hostURL = "";

        if(inputMode == InputMode.objectName)
        {
            form.AddField("asset_name", objName);
            objectFetchURL = objZipFetchURL;
            hostURL = predefinedObjHostURL;
        }
        else if(inputMode == InputMode.prompt)
        {
            form.AddField("prompt", objName);
            objectFetchURL = text23DGenURL;
            hostURL = text23DHostURL;
        }
        
        using (var w = UnityWebRequest.Post(objectFetchURL, form))
        {
            Debug.Log(objectFetchURL);
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success) {
                Debug.Log(w.error);
                PrintErrorToScreen("Something went wrong while trying to get the object zipfile URL.");
                ResetSceneOnFail();
                yield break;
            }
            else {
                Debug.Log("finished");
                zipURL = JsonUtility.FromJson<ZipURL>(w.downloadHandler.text);

                // now download the file
                zipPath = Path.Combine(Application.persistentDataPath, zipURL.asset_name+".zip");
                statusTextVariable.text = "Downloading the compressed object...";
                using(UnityWebRequest www = UnityWebRequest.Get(hostURL + zipURL.zip_file))
                {

                    yield return www.SendWebRequest();
                    if(www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                        PrintErrorToScreen("Something went wrong while trying to download object zip file.");
                        ResetSceneOnFail();
                        yield return null;
                    }
                    else
                    {
                        byte[] result = www.downloadHandler.data;
                        Debug.Log(result.Length / (1024 * 1024));
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

        

        try
        {
            // extract file
            string extractPath = Application.persistentDataPath;
            statusTextVariable.text = "Extracting the compressed object...";
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
        catch
        {
            Debug.LogError("failed to extract downloaded zip file.");
            PrintErrorToScreen("failed to extract downloaded object zip file.");
            File.Delete(zipPath);
            ResetSceneOnFail();
            yield break;
        }
    }

    private void OnApplicationQuit() {
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

    public void DownloadExistingObject(){
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.objectName));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for(int i=0; i<errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }

    public void GenerateAndDownloadObject()
    {
        StartCoroutine(RenderDownloadedMesh(objectName, InputMode.prompt));
        var errorCount = errorScrollbarContainer.transform.childCount;
        for (int i = 0; i < errorCount; i++)
        {
            Destroy(errorScrollbarContainer.transform.GetChild(i).gameObject);
        }
    }
}