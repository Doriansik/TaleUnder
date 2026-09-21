using UnityEngine;
using SaintsField;

public class Billboard2D : MonoBehaviour
{
    [Separator("Camera Setup")]
    public Camera targetCamera;

    void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null) transform.forward = targetCamera.transform.forward;
    }
}