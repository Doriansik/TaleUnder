using UnityEngine;
using SaintsField;

public class SceneCameraConfig : MonoBehaviour
{
    [Separator("Scene Default Settings")]
    public bool isSceneStatic = false;
    public Transform staticCameraPosition;

    [Separator("Camera Bounds (Follow Mode)")]
    public bool useCameraBounds = false;
    [Tooltip("Assign a BoxCollider (IsTrigger) here to define the physical limits of the room.")]
    public BoxCollider boundsArea;
    [Tooltip("Distance from the collider edges where the camera center will stop. Tweak this until the screen edge matches the room wall.")]
    public Vector3 boundsPadding = new Vector3(8f, 0f, 5f);

    [Separator("Editor Testing")]
    public Transform editorDebugSpawn;

    void Start()
    {
        if (SceneDirector.Instance != null && !SceneDirector.Instance.isTransitioning)
        {
            if (editorDebugSpawn != null)
            {
                SceneDirector.Instance.playerTarget.position = editorDebugSpawn.position;
            }

            ApplyConfigNow();

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.ReturnToDefault(CameraTransitionType.Instant, 0f);
            }
        }
    }

    public void ApplyConfigNow()
    {
        if (CameraManager.Instance == null) return;

        Vector3 targetPos = staticCameraPosition != null ? staticCameraPosition.position : Vector3.zero;
        Bounds bounds = (useCameraBounds && boundsArea != null) ? boundsArea.bounds : new Bounds();

        CameraManager.Instance.ApplySceneSettings(isSceneStatic, targetPos, useCameraBounds, bounds, boundsPadding);
    }
}