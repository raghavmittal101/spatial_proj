using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Handles all network operations for the application
    /// </summary>
    public class NetworkService : INetworkService
    {
        private readonly ILogger _logger;

        public NetworkService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerator DownloadTexture(string url, Action<Texture2D> onSuccess, Action<string> onFailure)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    onSuccess?.Invoke(texture);
                }
                else
                {
                    string errorMessage = $"Error downloading texture: {request.error}\nURL: {url}";
                    _logger.LogError(errorMessage);
                    onFailure?.Invoke(errorMessage);
                }
            }
        }

        public IEnumerator SendPostRequest(string url, WWWForm form, Action<string> onSuccess, Action<string> onFailure)
        {
            using (var request = UnityWebRequest.Post(url, form))
            {
                _logger.LogInfo($"Sending POST request to: {url}");
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorMessage = $"POST request failed: {request.error}";
                    _logger.LogError(errorMessage);
                    onFailure?.Invoke(errorMessage);
                }
                else
                {
                    _logger.LogInfo("POST request completed successfully");
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
            }
        }

        public IEnumerator DownloadFile(string url, string destinationPath, Action onSuccess, Action<string> onFailure, Action<float> onProgress = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Set up download progress tracking
                request.downloadHandler = new DownloadHandlerBuffer();

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    onProgress?.Invoke(request.downloadProgress);
                    yield return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorMessage = $"Download failed: {request.error}";
                    _logger.LogError(errorMessage);
                    onFailure?.Invoke(errorMessage);
                    yield break;
                }

                try
                {
                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                }

                catch (Exception ex)
                {
                    string errorMessage = $"File operation error: {ex.Message}";
                    _logger.LogError(errorMessage);
                    onFailure?.Invoke(errorMessage);
                }

                // Write the file
                byte[] result = request.downloadHandler.data;
                    _logger.LogInfo($"Download size: {result.Length / (1024 * 1024)} MB");
                

                    using (FileStream fileStream = File.Open(destinationPath, FileMode.OpenOrCreate))
                    {
                        if (!fileStream.CanWrite)
                        {
                            string errorMessage = "Unable to write to the destination path";
                            _logger.LogError(errorMessage);
                            onFailure?.Invoke(errorMessage);
                            yield break;
                        }

                        fileStream.Seek(0, SeekOrigin.Begin);
                        Task writeTask = fileStream.WriteAsync(result, 0, result.Length);

                        while (!writeTask.IsCompleted)
                        {
                            yield return null;
                        }

                        if (writeTask.IsFaulted)
                        {
                            string errorMessage = $"File write error: {writeTask.Exception?.Message}";
                            _logger.LogError(errorMessage);
                            onFailure?.Invoke(errorMessage);
                            yield break;
                        }
                    }

                    _logger.LogInfo($"File successfully downloaded to: {destinationPath}");
                    onSuccess?.Invoke();
                
                
            }
        }
    }
}