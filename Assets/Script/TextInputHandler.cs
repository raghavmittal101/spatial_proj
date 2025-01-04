using UnityEngine;
using UnityEngine.UI;

public class TextInputHandler : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField promptInputField;
    [SerializeField] private MainScript mainScript;
    [SerializeField] private Button submitButton;
    private UnityEngine.Events.UnityAction buttonOnClickAction;

    private void Start()
    {
        buttonOnClickAction += ProcessSelectedItem;
        submitButton.onClick.AddListener(buttonOnClickAction);
    }

    private void ProcessSelectedItem()
    {
        var userInput = promptInputField.text;
        mainScript.ProcessUserInput(userInput);
    }
}
