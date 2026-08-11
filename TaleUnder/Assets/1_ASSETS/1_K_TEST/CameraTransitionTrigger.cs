using UnityEngine;
using PrimeTween;
using SaintsField;

public class CameraTransitionTrigger : MonoBehaviour
{
    [Separator("Camera Setup")]

    [Required]
    public Transform targetCameraSetup;

    [Range(0.1f, 10f)]
    public float transitionDuration = 2f;

    public Ease transitionEase = Ease.InOutSine;

    private bool hasTriggered = false;
    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            Tween.Position(mainCameraTransform, targetCameraSetup.position, transitionDuration, transitionEase);
            Tween.Rotation(mainCameraTransform, targetCameraSetup.rotation, transitionDuration, transitionEase);
        }
    }
}