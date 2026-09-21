using UnityEngine;
using PrimeTween;
using System;
using TMPro;

public class SpotHitMinigame : MonoBehaviour, ICombatMinigame
{
    public CanvasGroup minigameGroup;
    public RectTransform leftSpawn;
    public RectTransform rightSpawn;
    public RectTransform centerTarget;
    public GameObject starPrefab;
    public TMP_Text feedbackText;

    public Action<float> OnMinigameEnd { get; set; }

    private Transform playerVisual;
    private SpriteRenderer playerSpriteRenderer;
    private Sprite originalSprite;
    private Transform currentTarget;
    private PlayerSkillSO currentSkillData;
    
    private RectTransform activeLeftStar;
    private RectTransform activeRightStar;
    private float targetBeatAbsolute;
    private bool isListeningForInput = false;

    public void StartMinigame(Transform targetEnemy, PlayerSkillSO skillData)
    {
        currentTarget = targetEnemy;
        currentSkillData = skillData;
        minigameGroup.alpha = 1f;

        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }

        if (playerVisual == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform visual = player.transform.Find("PlayerVisual");
                playerVisual = visual != null ? visual : player.transform;
                playerSpriteRenderer = playerVisual.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer != null) originalSprite = playerSpriteRenderer.sprite;
            }
        }

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

        if (playerVisual != null)
        {
            Tween.PunchScale(playerVisual, new Vector3(0.1f, 0.1f, 0.1f), duration, 1);
            Tween.PunchLocalPosition(playerVisual, new Vector3(0f, 0.2f, 0f), duration, 1);
        }

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
        isListeningForInput = false;
        float damageMultiplier = 0f;
        string popupMessage = "MISS!";
        Color popupColor = Color.gray;

        if (!forcedMiss)
        {
            float songPos = RhythmManager.Instance.GetCurrentBeatPosition();
            float distance = Mathf.Abs(targetBeatAbsolute - songPos);

            if (distance <= 0.15f)
            {
                damageMultiplier = 1f;
                popupMessage = "PERFECT!";
                popupColor = Color.yellow;
            }
            else if (distance <= 0.35f)
            {
                damageMultiplier = 0.5f;
                popupMessage = "GOOD!";
                popupColor = Color.green;
            }
        }
        
        TriggerFeedback(damageMultiplier > 0f, popupMessage, popupColor);
        Tween.Delay(1.5f).OnComplete(() => EndMinigame(damageMultiplier));
    }

    private void TriggerFeedback(bool isHit, string message, Color textColor)
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = message;
            feedbackText.color = textColor;
            Tween.PunchScale(feedbackText.transform, new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 1);
        }

        if (currentSkillData == null) return;

        if (isHit)
        {
            if (currentSkillData.hitSFX != null && CombatManager.Instance != null) 
                CombatManager.Instance.PlaySFX(currentSkillData.hitSFX);
            
            if (playerSpriteRenderer != null && currentSkillData.successPose != null) 
                playerSpriteRenderer.sprite = currentSkillData.successPose;
                
            if (currentSkillData.hitVFXPrefab != null && currentTarget != null) 
                Instantiate(currentSkillData.hitVFXPrefab, currentTarget.position, Quaternion.identity);
        }
        else
        {
            if (currentSkillData.missSFX != null && CombatManager.Instance != null) 
                CombatManager.Instance.PlaySFX(currentSkillData.missSFX);
                
            if (playerSpriteRenderer != null && currentSkillData.failPose != null) 
                playerSpriteRenderer.sprite = currentSkillData.failPose;
        }
    }

    private void EndMinigame(float multiplier)
    {
        if (playerSpriteRenderer != null && originalSprite != null)
        {
            playerSpriteRenderer.sprite = originalSprite;
        }

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (activeLeftStar != null) Destroy(activeLeftStar.gameObject);
        if (activeRightStar != null) Destroy(activeRightStar.gameObject);

        minigameGroup.alpha = 0f;
        OnMinigameEnd?.Invoke(multiplier);
        Destroy(gameObject);
    }
}