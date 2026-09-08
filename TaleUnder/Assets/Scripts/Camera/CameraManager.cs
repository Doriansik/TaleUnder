using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using SaintsField;

public enum CameraTransitionType { Instant, Smooth, Fade }
public enum FadeStyle { SolidColor, SwipeLeft, CircleOut }

[System.Serializable]
public class CameraSetup
{
    [Required] public Transform targetTransform;
    public CameraTransitionType transitionType = CameraTransitionType.Smooth;
    public FadeStyle fadeStyle = FadeStyle.SolidColor;

    [Range(0.1f, 5f)] public float duration = 1f;
    public Ease easeType = Ease.InOutSine;
    public Color fadeColor = Color.black;

    [Separator("Lens Settings")]
    public bool modifyFOV = false;
    [Range(10f, 120f)] public float targetFOV = 60f;
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Separator("Core References")]
    [Required] public Camera mainCamera;
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public RectTransform circleTransitionRect;

    [Separator("Player Tracking (Default State)")]
    public bool defaultIsStatic = false;

    [Required] public Transform playerTarget;
    [Range(1f, 20f)] public float followSpeed = 5f;

    private Vector3 defaultOffset;
    private Vector3 defaultRotation;
    private Vector3 defaultStaticPosition;

    private bool useCameraBounds = false;
    private Bounds currentBounds;
    private Vector3 boundsPadding;

    private Sequence currentSequence;
    private Transform cameraTransform;
    private bool isFollowingPlayer = true;
    private bool isStaticSetupActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cameraTransform = mainCamera.transform;
    }

    void Start()
    {
        if (playerTarget != null)
        {
            defaultOffset = cameraTransform.position - playerTarget.position;
        }
        defaultRotation = cameraTransform.eulerAngles;
        defaultStaticPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (isFollowingPlayer && playerTarget != null && !isStaticSetupActive && !defaultIsStatic)
        {
            Vector3 targetPos = playerTarget.position + defaultOffset;

            if (useCameraBounds)
            {
                targetPos = CalculateBoundPosition(targetPos);
            }

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, followSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(defaultRotation), followSpeed * Time.deltaTime);
        }
    }

    private Vector3 CalculateBoundPosition(Vector3 targetPos)
    {
        float minX = currentBounds.min.x + boundsPadding.x;
        float maxX = currentBounds.max.x - boundsPadding.x;
        float minY = currentBounds.min.y + boundsPadding.y;
        float maxY = currentBounds.max.y - boundsPadding.y;
        float minZ = currentBounds.min.z + boundsPadding.z;
        float maxZ = currentBounds.max.z - boundsPadding.z;

        if (minX > maxX) minX = maxX = currentBounds.center.x;
        if (minY > maxY) minY = maxY = currentBounds.center.y;
        if (minZ > maxZ) minZ = maxZ = currentBounds.center.z;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        return targetPos;
    }

    public void ApplySetup(CameraSetup setup, bool stayStatic = false)
    {
        if (setup.targetTransform == null) return;

        isFollowingPlayer = false;
        isStaticSetupActive = stayStatic;

        currentSequence.Stop();
        currentSequence = Sequence.Create();

        if (setup.modifyFOV)
        {
            currentSequence.Group(Tween.CameraFieldOfView(mainCamera, setup.targetFOV, setup.duration, setup.easeType));
        }

        switch (setup.transitionType)
        {
            case CameraTransitionType.Instant:
                cameraTransform.position = setup.targetTransform.position;
                cameraTransform.rotation = setup.targetTransform.rotation;
                break;
            case CameraTransitionType.Smooth:
                currentSequence.Chain(Tween.Position(cameraTransform, setup.targetTransform.position, setup.duration, setup.easeType));
                currentSequence.Group(Tween.Rotation(cameraTransform, setup.targetTransform.rotation, setup.duration, setup.easeType));
                break;
            case CameraTransitionType.Fade:
                if (fadeImage != null) fadeImage.color = setup.fadeColor;
                if (fadeCanvasGroup != null)
                {
                    currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 1f, setup.duration / 2f));
                    currentSequence.ChainCallback(() =>
                    {
                        cameraTransform.position = setup.targetTransform.position;
                        cameraTransform.rotation = setup.targetTransform.rotation;
                    });
                    currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 0f, setup.duration / 2f));
                }
                break;
        }
    }

    public void ReturnToDefault(CameraTransitionType returnType, float duration = 1f)
    {
        isStaticSetupActive = false;
        currentSequence.Stop();
        currentSequence = Sequence.Create();

        Vector3 targetPos = defaultIsStatic ? defaultStaticPosition : (playerTarget.position + defaultOffset);

        if (!defaultIsStatic && useCameraBounds)
        {
            targetPos = CalculateBoundPosition(targetPos);
        }

        Quaternion targetRot = Quaternion.Euler(defaultRotation);

        switch (returnType)
        {
            case CameraTransitionType.Instant:
                cameraTransform.position = targetPos;
                cameraTransform.rotation = targetRot;
                isFollowingPlayer = true;
                break;
            case CameraTransitionType.Smooth:
                currentSequence.Chain(Tween.Position(cameraTransform, targetPos, duration, Ease.InOutSine));
                currentSequence.Group(Tween.Rotation(cameraTransform, targetRot, duration, Ease.InOutSine));
                currentSequence.ChainCallback(() => isFollowingPlayer = true);
                break;
            case CameraTransitionType.Fade:
                if (fadeCanvasGroup != null)
                {
                    currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 1f, duration / 2f));
                    currentSequence.ChainCallback(() =>
                    {
                        cameraTransform.position = targetPos;
                        cameraTransform.rotation = targetRot;
                    });
                    currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 0f, duration / 2f));
                    currentSequence.ChainCallback(() => isFollowingPlayer = true);
                }
                break;
        }

        currentSequence.Group(Tween.CameraFieldOfView(mainCamera, 60f, duration, Ease.InOutSine));
    }

    public void SetDefaultStaticState(bool makeStatic)
    {
        defaultIsStatic = makeStatic;
        if (defaultIsStatic)
        {
            defaultStaticPosition = cameraTransform.position;
        }
    }

    public void ApplySceneSettings(bool isStatic, Vector3 staticPos, bool hasBounds, Bounds bounds, Vector3 padding)
    {
        defaultIsStatic = isStatic;
        defaultStaticPosition = staticPos;
        useCameraBounds = hasBounds;
        currentBounds = bounds;
        boundsPadding = padding;
    }
}