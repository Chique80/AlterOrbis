using Cinemachine;
using Mirror;
using UnityEngine;

namespace Utility
{
    public class AutoCameraSetup : NetworkBehaviour
    {
        [SerializeField] public CinemachineVirtualCamera virtualCamera;
        [SerializeField] public AudioListener audioListener;

        public override void OnStartClient()
        {
            virtualCamera.gameObject.SetActive(false);
            audioListener.gameObject.SetActive(false);
        }

        public override void OnStartLocalPlayer()
        {
            virtualCamera.gameObject.SetActive(true);
            audioListener.gameObject.SetActive(true);
        }
    }
}