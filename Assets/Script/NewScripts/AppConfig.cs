using UnityEngine;
using System.IO;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Contains configuration settings for the application
    /// </summary>
    [CreateAssetMenu(fileName = "AppConfig", menuName = "ThreeDGeneration/App Config")]
    public class AppConfig : ScriptableObject
    {
        [Header("API Endpoints")]
        [SerializeField] private string _predefinedObjHostURL = "http://100.64.0.1:5000/";
        [SerializeField] private string _text23DHostURL = "http://api.example.com/";
        [SerializeField] private string _apiVersion = "v0";

        [Header("API Paths")]
        [SerializeField] private string _objFileFetchPath = "/download-asset/";
        [SerializeField] private string _objZipFetchPath = "/download-asset-compressed/";
        [SerializeField] private string _text23DGenPath = "/text-2-3d/";
        [SerializeField] private string _image23DGenPath = "/img-2-3d/";
        [SerializeField] private string _text2ImageGenPath = "/text-2-img/";

        [Header("File Settings")]
        [SerializeField] private string _downloadFolder = "Downloads";

        [Header("Rendering Settings")]
        [SerializeField] private float _defaultObjectScale = 1.5f;
        [SerializeField] private float _defaultObjectYOffset = 0.05f;

        public string PredefinedObjHostURL => _predefinedObjHostURL;
        public string Text23DHostURL => _text23DHostURL;
        public string ApiVersion => _apiVersion;

        public string ObjFileFetchURL => $"{_predefinedObjHostURL}{_apiVersion}{_objFileFetchPath}";
        public string ObjZipFetchURL => $"{_predefinedObjHostURL}{_apiVersion}{_objZipFetchPath}";
        public string Text23DGenURL => $"{_text23DHostURL}{_apiVersion}{_text23DGenPath}";
        public string Image23DGenURL => $"{_text23DHostURL}{_apiVersion}{_image23DGenPath}";
        public string Text2ImageGenURL => $"{_text23DHostURL}{_apiVersion}{_text2ImageGenPath}";

        public string DownloadFolder => Path.Combine(Application.persistentDataPath, _downloadFolder);

        public float DefaultObjectScale => _defaultObjectScale;
        public float DefaultObjectYOffset => _defaultObjectYOffset;
    }
}