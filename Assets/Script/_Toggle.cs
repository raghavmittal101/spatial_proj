using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class _ToggleMaker : MonoBehaviour
{
    private bool toggleState;
    Button button;
    private TMPro.TMP_Text text;
    private Image image;
    public UnityEvent<Transform> onSelect;
    [HideInInspector] public Transform _transform { get; }
    public UnityEvent<bool> onValueChanged;
    [HideInInspector]
    public bool allowTurnOff = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = transform.GetComponent<Button>();
        button.onClick.AddListener(Toggle);
        text = button.GetComponentInChildren<TMPro.TMP_Text>();
        image = button.GetComponent<Image>();
    }
    void Toggle()
    {
        
        if (toggleState)
        {
            if (allowTurnOff)
            {
                UnSelect();
            }
        }
        else
        {
            Select();
        }
    }
    public bool IsSelected()
    {
        return toggleState;
    }
    public void UnSelect()
    {
        toggleState = false;
        onValueChanged.Invoke(toggleState);
        if (image != null)
            image.color = button.colors.normalColor;
        else if (text != null)
            text.color = button.colors.normalColor;
        
    }
    public void Select()
    {
        onSelect.Invoke(transform);
        toggleState = true;
        onValueChanged.Invoke(toggleState);
        if (image != null)
            image.color = button.colors.pressedColor;
        else if (text != null)
            text.color = button.colors.pressedColor;
        
    }
}
