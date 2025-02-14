using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class GrabbableObjUpdater : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject _grabbable;
    HandGrabInteractable handGrabInteractable;
    GrabInteractable grabInteractable;
    Grabbable grabbable;


    public void Start()
    {
        grabbable = _grabbable.GetComponent<Grabbable>();
        handGrabInteractable = _grabbable.GetComponent<HandGrabInteractable>();
        grabInteractable = _grabbable.GetComponent<GrabInteractable>();
        //grabFreeTransformer = grabObj.GetComponent<GrabFreeTransformer>();

        var rigidbody = GetComponent<Rigidbody>();
        grabbable.InjectOptionalRigidbody(rigidbody);
        Transform transform = GetComponent<Transform>();
        grabbable.InjectOptionalTargetTransform(transform);
        handGrabInteractable.InjectRigidbody(rigidbody);
        grabInteractable.InjectRigidbody(rigidbody);
    }
}
