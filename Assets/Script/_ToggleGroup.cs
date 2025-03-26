using System.Collections.Generic;
using UnityEngine;

public class _ToggleGroup : MonoBehaviour
{
    private _ToggleMaker[] toggles;
    [SerializeField] bool allowTurnOff;
    public _ToggleMaker currentActiveToggle { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentActiveToggle = null;
        toggles = transform.GetComponentsInChildren<_ToggleMaker>();
        foreach(var toggle in toggles)
        {
            toggle.onSelect.AddListener(UnselectOtherOptions);
            toggle.allowTurnOff = allowTurnOff;
        }
    }
    public void UnselectOtherOptions(Transform toggleTransform)
    {
        Debug.Log("UnselectOtherOptions running");
        // toggleTransform.GetComponent<_ToggleMaker>().Select();
        foreach (var i in toggles)
        {
            if(i != toggleTransform.GetComponent<_ToggleMaker>())
            {
                
                i.UnSelect();
            }
        }
        currentActiveToggle = toggleTransform.GetComponent<_ToggleMaker>();
    }
}
