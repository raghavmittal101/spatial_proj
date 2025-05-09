using UnityEngine;

public class DrawRendererBounds : MonoBehaviour
{
    [SerializeField] GameObject boundingCube;

    public void ResizeCollider()
    {
        var existingScale = boundingCube.transform.localScale;
        boundingCube.transform.localScale = Vector3.one;

        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        

        var bcollider = boundingCube.GetComponent<BoxCollider>();
        bcollider.center = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
        bcollider.size = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);

        boundingCube.transform.localScale = existingScale;
    }
}