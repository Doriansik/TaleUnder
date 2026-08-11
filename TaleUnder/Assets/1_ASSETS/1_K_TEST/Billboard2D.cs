using UnityEngine;
using SaintsField;

public class Billboard2D : MonoBehaviour
{
    [Separator("Camera Setup")]

    [InfoBox("Drag and drop your camera here. If left empty, the script will try to find a camera with the 'MainCamera' tag.")]
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("Billboard2D cannot find the camera! Assign it manually or tag your camera as 'MainCamera'.", this);
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.rotation = targetCamera.transform.rotation;
        }
    }
}