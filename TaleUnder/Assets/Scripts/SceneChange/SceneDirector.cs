using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PrimeTween;
using SaintsField;
using System.Collections;
using Unity.Cinemachine;

public enum FadeStyle { SolidColor, CircleOut, Custom }

public class SceneDirector : MonoBehaviour
{
    public static SceneDirector Instance;

    [Separator("Core References")]
    [Required] public Transform playerTarget;

    public bool isTransitioning { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TransitionToScene(SceneReferenceSO sceneRef, SpawnPointKeySO spawnKey, FadeStyle style, Color color, float duration)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionRoutine(sceneRef, spawnKey, style, color, duration));
        }
    }

    private IEnumerator TransitionRoutine(SceneReferenceSO sceneRef, SpawnPointKeySO spawnKey, FadeStyle style, Color color, float duration)
    {
        isTransitioning = true;
        CameraManager cam = CameraManager.Instance;

        if (cam != null)
        {
            if (cam.fadeCanvasGroup != null) cam.fadeCanvasGroup.alpha = 1f;

            if (style == FadeStyle.SolidColor)
            {
                if (cam.circleTransitionRect != null) cam.circleTransitionRect.gameObject.SetActive(false);
                
                if (cam.fadeImage != null)
                {
                    cam.fadeImage.gameObject.SetActive(true);
                    cam.fadeImage.color = color;
                }
                
                if (cam.fadeCanvasGroup != null)
                {
                    cam.fadeCanvasGroup.alpha = 0f;
                    yield return Tween.Alpha(cam.fadeCanvasGroup, 1f, duration).ToYieldInstruction();
                }
            }
            else if (style == FadeStyle.CircleOut)
            {
                if (cam.fadeImage != null) cam.fadeImage.gameObject.SetActive(false);
                
                if (cam.circleTransitionRect != null)
                {
                    cam.circleTransitionRect.gameObject.SetActive(true);
                    Image circleImg = cam.circleTransitionRect.GetComponent<Image>();
                    if (circleImg != null) circleImg.color = color;
                    
                    cam.circleTransitionRect.localScale = Vector3.zero;
                    yield return Tween.Scale(cam.circleTransitionRect, Vector3.one * 50f, duration, Ease.InSine).ToYieldInstruction();
                }
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneRef.sceneName);
        while (!asyncLoad.isDone) yield return null;

        SpawnPoint[] allSpawns = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude);

        foreach (SpawnPoint sp in allSpawns)
        {
            if (sp.spawnKey == spawnKey)
            {
                playerTarget.position = sp.transform.position;
                break;
            }
        }

        if (cam != null)
        {
            SceneCameraConfig config = FindAnyObjectByType<SceneCameraConfig>();
            if (config != null) config.ApplyConfigNow();

            if (style == FadeStyle.SolidColor && cam.fadeCanvasGroup != null)
            {
                yield return Tween.Alpha(cam.fadeCanvasGroup, 0f, duration).ToYieldInstruction();
            }
            else if (style == FadeStyle.CircleOut && cam.circleTransitionRect != null)
            {
                yield return Tween.Scale(cam.circleTransitionRect, Vector3.zero, duration, Ease.OutSine).ToYieldInstruction();
                cam.circleTransitionRect.gameObject.SetActive(false);
            }
        }

        isTransitioning = false;
    }
}