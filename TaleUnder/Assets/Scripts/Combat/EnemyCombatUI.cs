using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using TMPro;

public class EnemyCombatUI : MonoBehaviour
{
    public CanvasGroup uiCanvasGroup;
    public Slider hpSlider;
    public TMP_Text hpText; 
    public TMP_Text emotionText; 

    private int maxHP;
    private int currentHP;
    private EmotionType currentEmotion;

    public void Initialize(int startingHP, EmotionType startEmotion = EmotionType.Neutral)
    {
        maxHP = startingHP;
        currentHP = startingHP;
        currentEmotion = startEmotion;

        if (hpSlider != null) 
        { 
            hpSlider.maxValue = maxHP; 
            hpSlider.value = currentHP; 
        }
        UpdateUIElements();

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void ShowUI()
    {
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.gameObject.SetActive(true);
            Tween.Alpha(uiCanvasGroup, 1f, 0.2f);
        }
    }

    public void HideUI()
    {
        if (uiCanvasGroup != null) 
        {
            Tween.Alpha(uiCanvasGroup, 0f, 0.2f).OnComplete(() => uiCanvasGroup.gameObject.SetActive(false));
        }
    }

    public void UpdateHP(int newHP)
    {
        currentHP = Mathf.Clamp(newHP, 0, maxHP);
        if (hpSlider != null) 
        {
            Tween.UISliderValue(hpSlider, currentHP, 0.3f, Ease.OutBounce);
        }
        UpdateUIElements();
    }

    public void SetEmotion(EmotionType newEmotion)
    {
        currentEmotion = newEmotion;
        UpdateUIElements();
    }

    private void UpdateUIElements()
    {
        if (hpText != null) hpText.text = $"{currentHP}/{maxHP}";
        if (emotionText != null) emotionText.text = currentEmotion.ToString().ToUpper();
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }
}