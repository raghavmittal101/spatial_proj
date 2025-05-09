using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Dummiesman;

namespace ThreeDGeneration.Core
{
    public class ImageGenerator : MonoBehaviour, IImageGenerator
    {
        private INetworkService _networkService;
        private ILogger _logger;
        private AppConfig _config;

        private void Awake()
        {
            _logger = new UnityLogger("ImageGenerator");
            _networkService = new NetworkService(_logger);
        }

        [Serializable]
        private class Text2ImageResponse
        {
            public string img_file;
            public string asset_name;
        }

        public IEnumerator GenerateImage(string prompt, Action onStart, Action<Texture2D, string> onSuccess, Action<string> onFailure, Action<string> onProgress = null)
        {
            onStart?.Invoke();
            onProgress?.Invoke("Preparing to generate image from text...");

            WWWForm form = new WWWForm();
            form.AddField("prompt", prompt);

            yield return _networkService.SendPostRequest(
                _config.Text2ImageGenURL,
                form,
                responseJson =>
                {
                    try
                    {
                        Text2ImageResponse response = JsonUtility.FromJson<Text2ImageResponse>(responseJson);
                        string imageUrl = _config.Text23DHostURL + response.img_file;
                        _logger.LogInfo($"Image generated successfully: {imageUrl}");
                        StartCoroutine(DownloadImageAndConvertToTexture2D(
                            imageUrl,
                            response.asset_name,
                            texture => { onSuccess?.Invoke(texture, response.asset_name); },
                            onFailure,
                            onProgress)
                            );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to parse image response: {ex.Message}");
                        onFailure?.Invoke($"Failed to process generated image: {ex.Message}");
                    }
                },
                error =>
                {
                    _logger.LogError($"Failed to generate image: {error}");
                    onFailure?.Invoke($"Failed to generate image: {error}");
                }
            );
        }

        private IEnumerator DownloadImageAndConvertToTexture2D(string imageURL, string asset_name, Action<Texture2D> onSuccess, Action<string> onFailure, Action<string> onProgress)
        {
            onProgress?.Invoke("Downloading the image file and converting it into texture ...");
            yield return _networkService.DownloadTexture(
                            imageURL,
                            texture => { onSuccess?.Invoke(texture); },
                            failure =>
                            {
                                _logger.LogError("Failed to download image as texture.");
                                onFailure?.Invoke("Failed to download image as texture.");
                            }
                        );
        }
    }
}
