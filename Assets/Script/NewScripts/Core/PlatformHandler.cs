using UnityEngine;

namespace ThreeDGeneration.Core
{
    public class PlatformHandler : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] GameObject unityCamera;
        [SerializeField] GameObject oculusCamera;

        [Header("Event System")]
        [SerializeField] GameObject unityEventSystem;
        [SerializeField] GameObject oculusEventSystem;

        void EnableEventSystem()
        {
#if UNITY_EDITOR
            unityEventSystem.SetActive(true);
            oculusEventSystem.SetActive(false);
#else
            unityEventSystem.SetActive(false);
            oculusEventSystem.SetActive(true);
            
#endif
        }

        void EnableCamera()
        {
#if UNITY_EDITOR
            unityCamera.SetActive(true);
            oculusCamera.SetActive(false);

#else
            unityCamera.SetActive(false);
            oculusCamera.SetActive(true);
#endif
        }

        public void Start()
        {
            EnableEventSystem();
            EnableCamera();
        }

    }
}
