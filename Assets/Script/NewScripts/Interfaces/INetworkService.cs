using System;
using System.Collections;
using UnityEngine;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Interface for network-related operations
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Downloads a texture from a URL
        /// </summary>
        /// <param name="url">The URL to download from</param>
        /// <param name="onSuccess">Called when download is successful with the texture</param>
        /// <param name="onFailure">Called when download fails with error message</param>
        IEnumerator DownloadTexture(string url, Action<Texture2D> onSuccess, Action<string> onFailure);

        /// <summary>
        /// Sends a POST request to a URL
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="form">Form data to send</param>
        /// <param name="onSuccess">Called when request is successful with response text</param>
        /// <param name="onFailure">Called when request fails with error message</param>
        IEnumerator SendPostRequest(string url, WWWForm form, Action<string> onSuccess, Action<string> onFailure);

        /// <summary>
        /// Downloads a file from a URL to a local path
        /// </summary>
        /// <param name="url">The URL to download from</param>
        /// <param name="destinationPath">Where to save the file</param>
        /// <param name="onSuccess">Called when download is successful</param>
        /// <param name="onFailure">Called when download fails with error message</param>
        /// <param name="onProgress">Called with download progress (0-1)</param>
        IEnumerator DownloadFile(string url, string destinationPath, Action onSuccess, Action<string> onFailure, Action<float> onProgress = null);
    }
}