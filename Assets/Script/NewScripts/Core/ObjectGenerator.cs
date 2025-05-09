using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Dummiesman;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Implementation of the IObjectGenerator interface for Image generation and 3D object generation
    /// </summary>
    public class ObjectGenerator : MonoBehaviour, IObjectGenerator
    {

        [SerializeField] private GameObject _grabbableObjContainerPrefab;

        private AppConfig _config;
        private INetworkService _networkService;
        private ILogger _logger;

        // Object placement references
        private GameObject _objectPlaceholder;

        private void Awake()
        {
            _logger = new UnityLogger("ObjectGenerator");
            _networkService = new NetworkService(_logger);
        }

        public void SetObjectPlaceholder(GameObject placeholder)
        {
            _objectPlaceholder = placeholder;
        }

        public IEnumerator GenerateFromText(string prompt, Action onStart, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress = null)
        {
            onStart?.Invoke();
            onProgress?.Invoke("Preparing to generate object from text...");

            WWWForm form = new WWWForm();
            form.AddField("prompt", prompt);

            // Send request to generate 3D model from text
            yield return _networkService.SendPostRequest(
                _config.Text23DGenURL,
                form,
                responseJson => StartCoroutine(ProcessGeneratedObjectResponse(responseJson, onSuccess, onFailure, onProgress)),
                error =>
                {
                    _logger.LogError($"Failed to generate 3D object from text: {error}");
                    onFailure?.Invoke($"Failed to generate 3D object: {error}");
                }
            );
        }

        public IEnumerator GenerateFromImage(string imageAssetName, Action onStart, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress = null)
        {
            onStart?.Invoke();
            onProgress?.Invoke("Preparing to generate object from asset...");

            WWWForm form = new WWWForm();
            form.AddField("asset_name", imageAssetName);

            string zipPath = Path.Combine(_config.DownloadFolder, $"{imageAssetName}.zip");
            string extractPath = Path.Combine(_config.DownloadFolder, imageAssetName);

            try
            {
                // Ensure clean state
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in cleanup before download: {ex.Message}");
                onFailure?.Invoke($"Error preparing asset: {ex.Message}");
                yield break;
            }

            // Download the zip file
            onProgress?.Invoke("Downloading asset...");
            yield return _networkService.DownloadFile(
                $"{_config.ObjZipFetchURL}?asset_name={imageAssetName}",
                zipPath,
                () => StartCoroutine(ExtractAndLoadObject(zipPath, extractPath, imageAssetName, onSuccess, onFailure, onProgress)),
                error =>
                {
                    _logger.LogError($"Failed to download asset: {error}");
                    onFailure?.Invoke($"Failed to download asset: {error}");
                }
            );

        }


        private IEnumerator ProcessGeneratedObjectResponse(string responseJson, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress)
        {

            ZipURLResponse response = JsonUtility.FromJson<ZipURLResponse>(responseJson);
            string zipUrl = _config.Text23DHostURL + response.zip_file;
            string assetName = response.asset_name;

            string zipPath = Path.Combine(_config.DownloadFolder, $"{assetName}.zip");
            string extractPath = Path.Combine(_config.DownloadFolder, assetName);
            try
            {
                // Ensure directories exist
                Directory.CreateDirectory(_config.DownloadFolder);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Failed to process generated object response: {ex.Message}");
                onFailure?.Invoke($"Failed to process generated object: {ex.Message}");
            }

            // Download the zip file
            onProgress?.Invoke("Downloading generated object...");
            yield return _networkService.DownloadFile(
                zipUrl,
                zipPath,
                () => StartCoroutine(ExtractAndLoadObject(zipPath, extractPath, assetName, onSuccess, onFailure, onProgress)),
                error =>
                {
                    _logger.LogError($"Failed to download generated object: {error}");
                    onFailure?.Invoke($"Failed to download generated object: {error}");
                },
                progress => onProgress?.Invoke($"Downloading: {progress * 100:F0}%")
            );
        }

        private IEnumerator ExtractAndLoadObject(string zipPath, string extractPath, string assetName, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress)
        {
            try
            {
                onProgress?.Invoke("Extracting object files...");

                // Extract the zip file
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract and load object: {ex.Message}");
                onFailure?.Invoke($"Failed to extract and load object: {ex.Message}");
                CleanupFiles(zipPath, extractPath);
            }
            // Load the OBJ file
            string objFilePath = Path.Combine(extractPath, $"{assetName}.obj");
            string mtlFilePath = Path.Combine(extractPath, $"{assetName}.mtl");

            onProgress?.Invoke("Loading 3D object...");

            if (!File.Exists(objFilePath))
            {
                throw new FileNotFoundException($"OBJ file not found at {objFilePath}");
            }

            // We need to defer loading to the main thread as Unity's mesh operations are not thread-safe
            yield return null;

            GameObject loadedObj = new OBJLoader().Load(objFilePath, mtlFilePath);
            GameObject finalObject = CreateGrabbableObject(loadedObj);

            onSuccess?.Invoke(finalObject);

        }

        private GameObject CreateGrabbableObject(GameObject loadedObj)
        {
            if (_objectPlaceholder == null)
            {
                throw new InvalidOperationException("Object placeholder not set!");
            }

            // Create a container for the loaded object
            GameObject grabbableObj = Instantiate(_grabbableObjContainerPrefab, _objectPlaceholder.transform);

            // Position the container
            grabbableObj.transform.position = new Vector3(
                _objectPlaceholder.transform.position.x,
                _objectPlaceholder.transform.position.y + _config.DefaultObjectYOffset,
                _objectPlaceholder.transform.position.z
            );

            // Scale the container
            grabbableObj.transform.localScale = new Vector3(
                _config.DefaultObjectScale,
                _config.DefaultObjectScale,
                _config.DefaultObjectScale
            );

            // Get the bounding cube within the grabbable object
            Transform boundingCubeTransform = grabbableObj.transform.GetChild(0).transform;

            // Calculate bounds of the loaded object
            Bounds boundsOfLoadedObj = GetChildRendererBounds(loadedObj);

            // Set the collider size based on the object bounds
            boundingCubeTransform.GetComponent<BoxCollider>().size = boundsOfLoadedObj.size;

            // Parent the loaded object to the bounding cube
            loadedObj.transform.parent = boundingCubeTransform.transform;
            loadedObj.transform.localPosition = Vector3.zero;
            loadedObj.transform.localScale = Vector3.one;

            return grabbableObj;
        }

        private Bounds GetChildRendererBounds(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                return bounds;
            }

            return new Bounds(Vector3.zero, Vector3.one);
        }

        private void CleanupFiles(string zipPath, string extractPath)
        {
            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to clean up files: {ex.Message}");
            }
        }

        [Serializable]
        private class ZipURLResponse
        {
            public string zip_file;
            public string asset_name;
        }


    }
}