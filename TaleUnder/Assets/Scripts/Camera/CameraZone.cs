using UnityEngine;
using SaintsField;

public class CameraZone : MonoBehaviour
{
    [Separator("Zone Camera Settings")]
    public CameraSetup zoneSetup;
    public bool isStaticRoomCamera = true;

    [Separator("Enter Settings")]
    public CameraTransitionType enterTransition = CameraTransitionType.Smooth;
    [Range(0.1f, 5f)] public float enterDuration = 1f;

    [Separator("Exit Settings")]
    public bool returnToDefaultOnExit = true;
    public CameraTransitionType exitTransition = CameraTransitionType.Smooth;
    [Range(0.1f, 5f)] public float exitDuration = 1f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager.Instance.ApplySetup(zoneSetup, isStaticRoomCamera, enterTransition, enterDuration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (returnToDefaultOnExit && other.CompareTag("Player"))
        {
            CameraManager.Instance.ReturnToDefault(exitTransition, exitDuration);
        }
    }
}