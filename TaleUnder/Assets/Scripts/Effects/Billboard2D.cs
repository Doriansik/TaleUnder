using UnityEngine;
using SaintsField;

public class Billboard2D : MonoBehaviour
{
    [Separator("Camera Setup")]
    public Camera targetCamera;

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            if (CameraManager.Instance != null && CameraManager.Instance.mainCamera != null)
            {
                targetCamera = CameraManager.Instance.mainCamera;
            }
            else
            {
                targetCamera = Camera.main;
            }

            return;
        }

        transform.rotation = targetCamera.transform.rotation;
    }
}