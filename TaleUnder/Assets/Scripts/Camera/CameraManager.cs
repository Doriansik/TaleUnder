using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using SaintsField;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Separator("Core References")]
    [Required] public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public RectTransform circleTransitionRect;

    private Sequence currentSequence;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayFadeTransition(float halfDuration, Color fadeColor)
    {
        if (fadeCanvasGroup == null) return;

        currentSequence.Stop();
        currentSequence = Sequence.Create();

        if (fadeImage != null) fadeImage.color = fadeColor;

        currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 1f, halfDuration));
        currentSequence.Chain(Tween.Alpha(fadeCanvasGroup, 0f, halfDuration));
    }
}