using UnityEngine;
using SaintsField;

public class CameraModeTrigger : MonoBehaviour
{
    [Separator("Mode Settings")]

    [Tooltip("If true, the camera will freeze in its current position. If false, it will start following the player again.")]
    public bool makeCameraStatic = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager.Instance.SetDefaultStaticState(makeCameraStatic);
        }
    }
}