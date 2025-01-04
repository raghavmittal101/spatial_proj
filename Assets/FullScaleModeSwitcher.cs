using UnityEngine;
using UnityEngine.UI;

public class FullScaleModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject boundingBox;
    [SerializeField] private Toggle toggleButton;
    [SerializeField] private Transform pointHighliter;
    [SerializeField] private OVRCameraRig oVRCameraRig;

    private Vector3 position, scale;
    private Quaternion rotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggleButton.onValueChanged.AddListener(_OnValueChanged);
    }

    private void _OnValueChanged(bool enabled)
    {
        var boxCollider = boundingBox.GetComponent<BoxCollider>(); // disable boxcollider to disable hand grabbing.
        var rigidBody = boundingBox.GetComponent<Rigidbody>();
        if (enabled)
        {
            boxCollider.enabled = false;
            rigidBody.isKinematic = false;
            scale = boundingBox.transform.localScale;
            rotation = boundingBox.transform.rotation;
            position = boundingBox.transform.position;
            boundingBox.transform.localScale = Vector3.one;
            boundingBox.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            //var cameraPosition = oVRCameraRig.centerEyeAnchor.position;
            //boundingBox.transform.SetPositionAndRotation(new Vector3((pointHighliter.transform.position.x - cameraPosition.x)*(1f/scale.x), 0f, pointHighliter.transform.position.z - cameraPosition.z) * (1f / scale.z), Quaternion.identity);

        }
        else
        {
            boxCollider.enabled = true;
            rigidBody.isKinematic = true;
            boundingBox.transform.localScale = scale;
            boundingBox.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
