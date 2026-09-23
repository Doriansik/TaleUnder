using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using Unity.Cinemachine;

public class OverworldHUDManager : MonoBehaviour
{
    public static OverworldHUDManager Instance;

    public PlayerProfileSO playerProfile;
    public PlayerMovement playerMovement;

    [Header("UI Groups")]
    public CanvasGroup mainHUDGroup;
    public CanvasGroup metronomeGroup;

    [Header("Cinematography")]
    public CinemachineCamera vcamOverworldMain;
    public CinemachineCamera vcamAFK;

    [Header("HUD Elements")]
    public TMP_Text hpText;
    public TMP_Text ppText;
    public TMP_Text emotionText;
    public TMP_Text levelText;
    public TMP_Text fansText;
    public Slider hpSlider;
    public Slider ppSlider;
    public TMP_Text starBitsText;
    public TMP_Text encoreStarsText;
    public TMP_Text currentTaskText;

    [Header("Settings")]
    public float idleTimeToShowHUD = 5f;
    public float hudHoldDuration = 2f;
    public KeyCode showHUDKey = KeyCode.Tab;
    public KeyCode toggleMetronomeKey = KeyCode.M;

    private float currentIdleTime = 0f;
    private float hudVisibleTimer = 0f;
    private bool isMainHUDVisible = false;
    private bool isMetronomeVisible = false;
    private bool isMetronomeToggledOn = false;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        if (mainHUDGroup != null) mainHUDGroup.alpha = 0f;
        if (metronomeGroup != null) metronomeGroup.alpha = 0f;

        UpdateHUDData();
        SetTask("Explore the city");
    }

    public void Update()
    {
        if (PlayerMovement.IsInCombat || PlayerMovement.IsMenuOpen)
        {
            SetHUDVisibility(false);
            if (PlayerMovement.IsInCombat) SetMetronomeVisibility(false);
            if (vcamAFK != null) vcamAFK.Priority = 0;
            currentIdleTime = 0f;
            return;
        }

        HandleHUDLogic();
        HandleMetronomeLogic();
    }

    private void HandleHUDLogic()
    {
        if (playerMovement != null && playerMovement.IsMoving)
        {
            currentIdleTime = 0f;
            if (vcamAFK != null) vcamAFK.Priority = 0;
        }
        else
        {
            currentIdleTime += Time.deltaTime;
        }

        if (Input.GetKey(showHUDKey))
        {
            hudVisibleTimer = hudHoldDuration;
        }
        else
        {
            if (hudVisibleTimer > 0f) hudVisibleTimer -= Time.deltaTime;
        }

        bool isAFK = currentIdleTime >= idleTimeToShowHUD;
        bool shouldShowHUD = (hudVisibleTimer > 0f) || isAFK;
        
        SetHUDVisibility(shouldShowHUD);

        if (vcamAFK != null)
        {
            vcamAFK.Priority = isAFK ? 20 : 0;
        }

        if (isMainHUDVisible)
        {
            UpdateHUDData();
        }
    }

    private void HandleMetronomeLogic()
    {
        if (Input.GetKeyDown(toggleMetronomeKey))
        {
            isMetronomeToggledOn = !isMetronomeToggledOn;
        }

        bool shouldShowMetronome = isMetronomeToggledOn || isMainHUDVisible;
        SetMetronomeVisibility(shouldShowMetronome);
    }

    private void SetHUDVisibility(bool show)
    {
        if (show == isMainHUDVisible || mainHUDGroup == null) return;
        
        isMainHUDVisible = show;
        Tween.Alpha(mainHUDGroup, show ? 1f : 0f, 0.3f, useUnscaledTime: true);
    }

    public void SetMetronomeVisibility(bool show)
    {
        if (show == isMetronomeVisible || metronomeGroup == null) return;
        
        isMetronomeVisible = show;
        Tween.Alpha(metronomeGroup, show ? 1f : 0f, 0.3f, useUnscaledTime: true);
    }

    public void SetTask(string taskName)
    {
        if (currentTaskText != null) currentTaskText.text = taskName;
    }

    public void UpdateHUDData()
    {
        if (playerProfile == null) return;

        if (hpText != null) hpText.text = $"{playerProfile.currentHP}/{playerProfile.GetTotalMaxHP()}";
        if (ppText != null) ppText.text = $"{playerProfile.currentPP}/{playerProfile.GetTotalMaxPP()}";
        if (emotionText != null) emotionText.text = playerProfile.currentEmotion.ToString().ToUpper();
        if (levelText != null) levelText.text = $"Lvl {playerProfile.currentLevel}";
        
        int requiredFans = playerProfile.GetFansRequiredForNextLevel();
        if (fansText != null) 
        {
            fansText.text = requiredFans > 0 ? $"{requiredFans - playerProfile.currentFans} fans till lvl up" : "MAX LEVEL";
        }

        if (hpSlider != null) 
        {
            hpSlider.maxValue = playerProfile.GetTotalMaxHP();
            hpSlider.value = playerProfile.currentHP;
        }
        
        if (ppSlider != null) 
        {
            ppSlider.maxValue = playerProfile.GetTotalMaxPP();
            ppSlider.value = playerProfile.currentPP;
        }

        if (starBitsText != null) starBitsText.text = playerProfile.starBits.ToString();
        if (encoreStarsText != null) encoreStarsText.text = playerProfile.encoreStars.ToString();
    }
}