using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class ImageFetcher : MonoBehaviour
{
    // 1. when a method runs, it send the prompt to generate image url
    // 2. get the image from the url
    // 3. populates a canvas with downloaded image put onto a button(named same as assetname) prefab as texture.
    // 4. clicking on the button calls a method for image to 3D generation
    // 5. for 3D generation, the name of the assetname is sent for 3D generation.
    // 6. the 3D generated file is downloaded from url
    // 7. rendered
    // points 5-7 are already covered in other scripts (streamOBJImporter)

    [SerializeField] GameObject imageButtonPrefab;
    [SerializeField] GameObject imageContainer;
    [SerializeField] GameObject imageContainerPanel;
    [SerializeField] Button generateImageButton;
    [SerializeField] MainScript mainScript;
    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] StreamOBJImporter streamOBJImporter;

    public string imageUrl = "https://sample-videos.com/img/Sample-png-image-1mb.png"; // Set your image URL here

    //public void ShowImageInUI(string url, string assetname)
    //{
    //    StartCoroutine(DownloadImageAndSetTheButton(url, assetname));
    //}

    private void Start()
    {
        generateImageButton.onClick.AddListener(ProcessTextToImage);
    }
    public IEnumerator DownloadImageAndSetTheButton(string url, string assetname, string prompt)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = ConvertTextureToSprite(texture);

                GameObject prefabObj = Instantiate(imageButtonPrefab, imageContainer.GetComponent<Transform>());
                prefabObj.name = assetname;
                streamOBJImporter.objectName = assetname;
                var button = prefabObj.GetComponent<Button>();
                button.onClick.AddListener(streamOBJImporter.GetMeshFromImgAssetName);
                var image = prefabObj.transform.Find("ImageHolder").GetComponent<Image>();
                if (button != null)
                {
                    image.sprite = sprite; // Assign the sprite to the button
                }

                var buttonText = prefabObj.transform.Find("Text (TMP)").GetComponent<TMPro.TMP_Text>();
                buttonText.text = prompt;
                imageContainerPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Error downloading image: " + request.error + "\n" +
                    "URL: " + url);
            }
        }
    }

    Sprite ConvertTextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }


    public void ProcessTextToImage()
    {
        mainScript.ProcessUserPromptForImage(inputField.text);
    }

    // spawn a button prefab and set it's image attribute to fetched image

    // link the onclick funtion of the button to a method which sends imagename as parameter to 3D mesh generation method
}
