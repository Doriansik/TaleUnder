using UnityEngine;
using PrimeTween;
using System;

public class SpotHitMinigame : MonoBehaviour
{
    public CanvasGroup minigameGroup;
    public RectTransform leftSpawn;
    public RectTransform rightSpawn;
    public RectTransform centerTarget;
    public GameObject starPrefab;

    private RectTransform activeLeftStar;
    private RectTransform activeRightStar;
    private float targetBeatAbsolute;
    private bool isListeningForInput = false;

    public Action<float> OnMinigameEnd; 

    public void StartMinigame()
    {
        minigameGroup.alpha = 1f;
        
        if (activeLeftStar != null) Destroy(activeLeftStar.gameObject);
        if (activeRightStar != null) Destroy(activeRightStar.gameObject);

        activeLeftStar = Instantiate(starPrefab, minigameGroup.transform).GetComponent<RectTransform>();
        activeRightStar = Instantiate(starPrefab, minigameGroup.transform).GetComponent<RectTransform>();

        activeLeftStar.anchoredPosition = leftSpawn.anchoredPosition;
        activeRightStar.anchoredPosition = rightSpawn.anchoredPosition;

        float currentSongPos = RhythmManager.Instance.GetCurrentBeatPosition();
        targetBeatAbsolute = Mathf.Round(currentSongPos) + 2f; 

        isListeningForInput = true;
    }

    public void ExecuteBeat(int currentMinigameBeat)
    {
        if (!isListeningForInput) return;

        float duration = RhythmManager.Instance.SecondsPerBeat * 0.4f;

        if (currentMinigameBeat == 1)
        {
            Vector2 midLeft = Vector2.Lerp(leftSpawn.anchoredPosition, centerTarget.anchoredPosition, 0.5f);
            Vector2 midRight = Vector2.Lerp(rightSpawn.anchoredPosition, centerTarget.anchoredPosition, 0.5f);
            Tween.UIAnchoredPosition(activeLeftStar, midLeft, duration, Ease.OutBack);
            Tween.UIAnchoredPosition(activeRightStar, midRight, duration, Ease.OutBack);
        }
        else if (currentMinigameBeat == 2)
        {
            Tween.UIAnchoredPosition(activeLeftStar, centerTarget.anchoredPosition, duration, Ease.OutBack);
            Tween.UIAnchoredPosition(activeRightStar, centerTarget.anchoredPosition, duration, Ease.OutBack);
        }
        else if (currentMinigameBeat >= 4)
        {
            EvaluateHit(forcedMiss: true);
        }
    }

    private void Update()
    {
        if (!isListeningForInput) return;

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            EvaluateHit(forcedMiss: false);
        }
    }

    private void EvaluateHit(bool forcedMiss)
    {
        float damageMultiplier = 0f;

        if (!forcedMiss)
        {
            float songPos = RhythmManager.Instance.GetCurrentBeatPosition();
            float distance = Mathf.Abs(targetBeatAbsolute - songPos);

            if (distance <= 0.15f) damageMultiplier = 1f;
            else if (distance <= 0.35f) damageMultiplier = 0.5f;
        }

        EndMinigame(damageMultiplier);
    }

    private void EndMinigame(float multiplier)
    {
        isListeningForInput = false;

        if (activeLeftStar != null) Destroy(activeLeftStar.gameObject);
        if (activeRightStar != null) Destroy(activeRightStar.gameObject);

        minigameGroup.alpha = 0f;
        OnMinigameEnd?.Invoke(multiplier);
    }
}