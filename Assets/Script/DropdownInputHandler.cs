using UnityEngine;
using UnityEngine.UI;

public class DropdownInputHandler : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown assetSelectorDropdown;
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
        var userInput = assetSelectorDropdown.options[assetSelectorDropdown.value].text;
        mainScript.ProcessUserInput(userInput);
    }


}
