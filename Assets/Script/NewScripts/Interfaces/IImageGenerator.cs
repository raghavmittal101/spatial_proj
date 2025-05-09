using System;
using System.Collections;
using UnityEngine;

namespace ThreeDGeneration.Core
{
    public interface IImageGenerator
    {
        /// <summary>
        /// Generates an image from a text prompt
        /// </summary>
        /// <param name="prompt">Text description of the image to generate</param>
        /// <param name="onStart">Called when generation starts</param>
        /// <param name="onSuccess">Called when generation completes successfully with image as texture2D and asset name</param>
        /// <param name="onFailure">Called when generation fails with error message</param>
        /// <param name="onProgress">Called with generation progress updates</param>
        IEnumerator GenerateImage(string prompt, Action onStart, Action<Texture2D, string> onSuccess, Action<string> onFailure, Action<string> onProgress = null);
    }

}
