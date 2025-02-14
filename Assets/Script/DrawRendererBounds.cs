using UnityEngine;

public class DrawRendererBounds : MonoBehaviour
{
    // Draws a wireframe box around the selected object,
    // indicating world space bounding volume.
    [SerializeField] GameObject boundingCube;


    //[SerializeField] GameObject boundsCube;


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
        //transform.localScale = transform.localScale;
        bcollider.center = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
        //bcollider.center = Vector3.zero;
        var scale = boundingCube.transform.localScale;
        bcollider.size = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);

        boundingCube.transform.localScale = existingScale;
        //parent = transform.parent;
        //transform.parent = transform;
    }

    //public void OnDrawGizmosSelected()
    //{
    //    var r = GetComponent<Renderer>();
    //    if (r == null)
    //        return;
    //    var bounds = r.bounds;
    //    Gizmos.matrix = Matrix4x4.identity;
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
    //}

    //private void OnDisable()
    //{
    //    transform.parent = parent;
    //}
}