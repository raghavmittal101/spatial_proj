using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ThreeDGeneration.Core;
namespace ThreeDGeneration.UI
{
    /// <summary>
    /// Manages all UI interactions and displays for the 3D generation system
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _errorTextPrefab;
        [SerializeField] private GameObject _errorScrollbarContainer;
        [SerializeField] private GameObject _statusPanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _imageGallery;
        [SerializeField] private TMP_InputField _inputField;

        [Header("Buttons")]
        [SerializeField] private Button _generateObjectButton;
        [SerializeField] private Button _downloadAssetButton;
        [SerializeField] private Button _generateImageButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject _downloadAssetButtonPrefab;

        [Header("AR/VR enablement elements")]
        [SerializeField] private GameObject envBoundingCube;
        [SerializeField] private GameObject objPrefabPlaceholderAR;
        [SerializeField] private GameObject objPrefabPlaceholderVR;

        [Header("Toggle")]
        [SerializeField] private Toggle VrArToggle;

        [Header("References")]
        [SerializeField] private IObjectGenerator _objectGenerator;
        [SerializeField] private IImageGenerator _imageGenerator;

        private Core.ILogger _logger;

        private void Awake()
        {
            _logger = new UnityLogger("UIManager");
        }

        private void Start()
        {
            // Initialize UI elements
            InitializeUI();

            // Set up button listeners
            SetupButtonListeners();
        }

        private void InitializeUI()
        {
            _statusText.text = "Load";
            _statusPanel.SetActive(false);
            _mainPanel.SetActive(true);
            _imageGallery.SetActive(false);

            ClearErrorMessages();
        }

        private void SetupButtonListeners()
        {
            //_generateObjectButton.onClick.AddListener(OnGenerateObjectClicked);
            //_downloadAssetButton.onClick.AddListener(OnDownloadAssetClicked);
            _generateImageButton.onClick.AddListener(OnGenerateImageClicked);

            VrArToggle.onValueChanged.AddListener(OnARModeToggleChange);
        }

        //private void OnGenerateObjectClicked()
        //{
        //    string prompt = _inputField.text;
        //    

        //ShowStatusPanel("Generating

        private void OnGenerateImageClicked()
        {
            var input = _inputField.text;
            if (string.IsNullOrEmpty(input))
            {
                DisplayErrorMessage("Please enter a text prompt");
                return;
            }
            _imageGenerator.GenerateImage(
                input,
                () => { _logger.LogInfo("Generate image button clicked."); },
                (texture2D, imageName) => {
                    CreateImageButton(texture2D, imageName, input);
                },
                error => { }
                );
        }

        private void DisplayErrorMessage(string message)
        {
            _logger.LogError(message);
        }

        private void CreateImageButton(Texture2D texture, string asset_name, string prompt)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)); ;

            GameObject prefabObj = Instantiate(_downloadAssetButtonPrefab, _imageGallery.GetComponent<Transform>());
            prefabObj.name = asset_name;
            var button = prefabObj.GetComponent<Button>();
            button.onClick.AddListener(delegate { OnGenerateObjectClicked(asset_name); });
            var image = prefabObj.transform.Find("ImageHolder").GetComponent<Image>();
            if (button != null)
            {
                image.sprite = sprite; // Assign the sprite to the button
            }

            var buttonText = prefabObj.transform.Find("Text (TMP)").GetComponent<TMPro.TMP_Text>();
            buttonText.text = prompt;
            _imageGallery.SetActive(true);
        }

        private void OnGenerateObjectClicked(string asset_name)
        {
            StartCoroutine(_objectGenerator.GenerateFromImage(
                asset_name,
                () => { _logger.LogInfo("Generate object button clicked"); },
                obj => { },
                error => {
                    _logger.LogError(error); }
                )
            );
        }

        private void ClearErrorMessages()
        {

        }

        private void OnARModeToggleChange(bool state)
        {
            if (enabled)
            {

                _objectGenerator.SetObjectPlaceholder(objPrefabPlaceholderAR);
                envBoundingCube.SetActive(false);
                objPrefabPlaceholderAR.SetActive(true);

            }
            else
            {
                _objectGenerator.SetObjectPlaceholder(objPrefabPlaceholderVR);
                envBoundingCube.SetActive(true);
                objPrefabPlaceholderAR.SetActive(false);
            }
        }

    }
}