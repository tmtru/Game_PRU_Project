using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{
    private CinemachineCamera vcam;

    private void Start()
    {
        vcam = GetComponent<CinemachineCamera>();

        if (vcam != null && PlayerController.Instance != null)
        {
            vcam.Follow = PlayerController.Instance.transform;
        }
    }

    private void Update()
    {
        if (vcam.Follow == null && PlayerController.Instance != null)
        {
            vcam.Follow = PlayerController.Instance.transform;
        }
    }

}
