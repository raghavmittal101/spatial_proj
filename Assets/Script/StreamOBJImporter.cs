using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO.Compression;
using System.Threading.Tasks;
using TMPro;

public class StreamOBJImporter : MonoBehaviour {
    public string objectName;
    [SerializeField] private string hostURL = "http://100.64.0.1:5000/";
    [SerializeField] private string apiVersion = "v0";
    
    [SerializeField] private TextMeshProUGUI statusTextVariable;
    [SerializeField] private GameObject statusPanel;
    private string objFileFetchURL;
    private string objZipFetchURL;
    private string texURL;
    private string mtlURL;
    private string objURL;
    private string zipPath;
    private string downloadedMeshDirPath;
    private MemoryStream obj_memStream;
    private MemoryStream mtl_memStream;
    private MemoryStream tex_memStream;

    [System.Serializable]
    public class ComponentURLs
    {
        public string obj_file;
        public string mtl_file;
        public string tex_file;
    }

    private ComponentURLs componentURLs;
    private Stream[] objComponentsMemStreams;

	void Start () { 
        objFileFetchURL = hostURL + apiVersion + "/download-asset/"; // Replace with your API endpoint
        objZipFetchURL =  hostURL + apiVersion + "/download-asset-compressed/";
        objComponentsMemStreams = new Stream[3];
	}

    private IEnumerator RenderDownloadedMesh(string objName){
        
        downloadedMeshDirPath = Path.Combine(Application.persistentDataPath, objName);
        string objfilePath = downloadedMeshDirPath + "/" + objName + ".obj";
        string mtlfilePath = downloadedMeshDirPath + "/" + objName + ".mtl";
        Debug.Log(objfilePath);
        yield return StartCoroutine(DownloadAndExtractZip(objName));
        statusTextVariable.text = "Processing the object for rendering...";
        var loadedObj = new OBJLoader().Load(objfilePath, mtlfilePath);
        statusPanel.SetActive(false);
    }

    private IEnumerator RenderStreamedMesh(string objName){
        yield return StartCoroutine(GetObjComponentsURLs(objName));
        yield return StartCoroutine(DownloadFile(hostURL + componentURLs.mtl_file, 0)); // mtl_file
        yield return StartCoroutine(DownloadFile(hostURL + componentURLs.tex_file, 1)); // tex_file
        yield return StartCoroutine(DownloadFile(hostURL + componentURLs.obj_file, 2)); // obj_file
        Debug.Log("now running ObjLoader......");
        var loadedObj = new OBJLoader().Load(objComponentsMemStreams[2], objComponentsMemStreams[0]);
    }

    private IEnumerator GetObjComponentsURLs(string objName)
    {
        WWWForm form = new WWWForm();
        form.AddField("asset_name", objName);
        using (var w = UnityWebRequest.Post(objFileFetchURL, form))
        {
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success) {
                Debug.Log(w.error);
            }
            else {
                Debug.Log("finished");
                componentURLs = JsonUtility.FromJson<ComponentURLs>(w.downloadHandler.text);
            }
        }
    }

    private IEnumerator DownloadFile(string fileURL, int streamsArrayIndex){
        Debug.Log("Downloading a file...");
        MemoryStream textStream;
        using(var www = new WWW(fileURL)){
            yield return www;
            textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.text));
            Debug.Log("downloading for "+streamsArrayIndex+" done! ");
            objComponentsMemStreams[streamsArrayIndex] = textStream;
            Debug.Log(Encoding.UTF8.GetByteCount(www.text));
        }
        
    }


    [System.Serializable]
    public class ZipURL
    {
        public string zip_file;
    }

    private ZipURL zipURL;

    // Downloads zip file from server to persistent path and returns the folder path
    private IEnumerator DownloadAndExtractZip(string objName){
        statusTextVariable.text = "Fetching the object URL...";
        WWWForm form = new WWWForm();
        form.AddField("asset_name", objName);
        using (var w = UnityWebRequest.Post(objZipFetchURL, form))
        {
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success) {
                Debug.Log(w.error);
            }
            else {
                Debug.Log("finished");
                zipURL = JsonUtility.FromJson<ZipURL>(w.downloadHandler.text);
            }
        }

        // now download the file
        zipPath = Path.Combine(Application.persistentDataPath, objName+".zip");
        statusTextVariable.text = "Downloading the compressed object...";
        using(UnityWebRequest www = UnityWebRequest.Get(hostURL + zipURL.zip_file)){

            yield return www.SendWebRequest();
            byte[] result = www.downloadHandler.data;
            Debug.Log(result.Length/(1024*1024));
            using (FileStream SourceStream = File.Open(zipPath, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                Task task = SourceStream.WriteAsync(result, 0, result.Length);
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }
        
        // extract file
        string extractPath = Application.persistentDataPath;
        statusTextVariable.text = "Extracting the compressed object...";
        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }

    private void OnApplicationQuit() {
        Directory.Delete (downloadedMeshDirPath, true);
        File.Delete (zipPath);
    }

    [System.Serializable]
    public class ConvertedFileURL
    {
        public string converted_compressed_file_url;
    }
    private ConvertedFileURL convertedFileURL;

    public IEnumerator UploadFileAndConvert(string filename, string filePath){
        // List formData = new List();
        byte[] filedata = File.ReadAllBytes(filePath);
        // files[0] = files[0].Replace(@“.",”");
        // formData.Add(new MultipartFormFileSection(“file”, bytes, files[0], “text/plain”));
        // StartCoroutine(UploadFile(formData));
        var convertTo3dUrl= hostURL + apiVersion + "/convert-to-3d/";
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", filedata, filename);
        using (var w = UnityWebRequest.Post(convertTo3dUrl, form))
        {
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success) {
                Debug.Log(w.error);
            }
            else {
                Debug.Log("finished");
                convertedFileURL = JsonUtility.FromJson<ConvertedFileURL>(w.downloadHandler.text);
            }
        }

    }

    public void UploadFileAndConvert(){
        StartCoroutine(UploadFileAndConvert("abc.png", "/home/raghav/Downloads/spatialsuiteSystem1.drawio (2).png"));
    }

    public void DownloadExistingObject(){
        StartCoroutine(RenderDownloadedMesh(objectName));
    }
}