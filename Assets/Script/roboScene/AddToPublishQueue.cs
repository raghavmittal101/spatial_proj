using UnityEngine;
using UnityEngine.UI;

public class AddToPublishQueue : MonoBehaviour
{
    public TrajectoryPlanner trajectoryPlanner;
    public _ToggleGroup toggleGroup;
    [SerializeField] Button categorySubmitButton;
    [SerializeField] GameObject catPanel;
    [SerializeField] Transform boundingCube;

    void Start()
    {
        categorySubmitButton.onClick.AddListener(AddObjToCategoryQueue);
    }

    void AddObjToCategoryQueue()
    {
        if (toggleGroup.currentActiveToggle != null)
        {
            Debug.Log(toggleGroup.transform.gameObject.name);
            var selectedCat = toggleGroup.currentActiveToggle.gameObject.transform.name;
            Debug.Log("selected cat name... " + selectedCat);

            switch (selectedCat)
            {
                case "fruit":
                    Debug.Log("Add target type: " + "fruit");
                    trajectoryPlanner.AddTargetToQueue(transform.gameObject, TargetType.Type.Fruit);
                    break;
                case "veg":
                    Debug.Log("Add target type: " + "veg");
                    trajectoryPlanner.AddTargetToQueue(transform.gameObject, TargetType.Type.Veg);
                    break;
                case "platform":
                    SpawnPlatform();
                    break;
            }

            catPanel.SetActive(false);
        }
    }

    void SpawnPlatform()
    {
        boundingCube.localScale = Vector3.one;
    }
}
