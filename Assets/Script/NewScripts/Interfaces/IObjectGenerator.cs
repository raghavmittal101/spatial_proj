using System;
using System.Collections;
using UnityEngine;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Interface for 3D object generation services
    /// </summary>
    public interface IObjectGenerator
    {
        /// <summary>
        /// Generates a 3D object from a text prompt
        /// </summary>
        /// <param name="prompt">Text description of the object to generate</param>
        /// <param name="onStart">Called when generation starts</param>
        /// <param name="onSuccess">Called when generation completes successfully with the GameObject</param>
        /// <param name="onFailure">Called when generation fails with error message</param>
        /// <param name="onProgress">Called with generation progress updates</param>
        IEnumerator GenerateFromText(string prompt, Action onStart, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress = null);

        /// <summary>
        /// Generates a 3D object from an existing asset name
        /// </summary>
        /// <param name="assetName">Name of the asset to generate from</param>
        /// <param name="onStart">Called when generation starts</param>
        /// <param name="onSuccess">Called when generation completes successfully with the GameObject</param>
        /// <param name="onFailure">Called when generation fails with error message</param>
        /// <param name="onProgress">Called with generation progress updates</param>
        IEnumerator GenerateFromImage(string assetName, Action onStart, Action<GameObject> onSuccess, Action<string> onFailure, Action<string> onProgress = null);

        void SetObjectPlaceholder(GameObject placeholder);
    }
}