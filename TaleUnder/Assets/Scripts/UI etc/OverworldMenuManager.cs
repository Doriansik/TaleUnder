using UnityEngine;
using PrimeTween;
using Unity.Cinemachine;

public class OverworldMenuManager : MonoBehaviour
{
    public static OverworldMenuManager Instance;

    [Header("Controls")]
    public KeyCode openMenuKey = KeyCode.Escape;

    [Header("UI Main Menu")]
    public CanvasGroup menuPanelGroup;
    public RectTransform menuLeftPanel;
    public float panelSlideOffset = -500f;
    
    [Header("UI Sub-Panels")]
    public CanvasGroup skillsPanel;
    public CanvasGroup inventoryPanel;
    public CanvasGroup relationsPanel;
    public CanvasGroup questsPanel;
    public CanvasGroup configPanel;

    [Header("Cinematography")]
    public CinemachineCamera vcamMenu;

    private bool isMenuOpen = false;
    private CanvasGroup activeSubPanel = null;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        if (menuPanelGroup != null)
        {
            menuPanelGroup.alpha = 0f;
            menuPanelGroup.gameObject.SetActive(false);
        }
        if (menuLeftPanel != null)
        {
            menuLeftPanel.anchoredPosition = new Vector2(panelSlideOffset, menuLeftPanel.anchoredPosition.y);
        }
        
        HideAllSubPanelsFast();
    }

    public void Update()
    {
        if (PlayerMovement.IsInCombat) return;

        if (Input.GetKeyDown(openMenuKey))
        {
            if (activeSubPanel != null)
            {
                CloseSubMenu();
            }
            else if (!isMenuOpen)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }
    }

    private void OpenMenu()
    {
        isMenuOpen = true;
        PlayerMovement.IsMenuOpen = true;
        
        Tween.Custom(AudioListener.volume, 0.5f, 0.3f, onValueChange: v => AudioListener.volume = v, useUnscaledTime: true);

        if (vcamMenu != null) vcamMenu.Priority = 30;

        if (menuPanelGroup != null)
        {
            menuPanelGroup.gameObject.SetActive(true);
            Tween.Alpha(menuPanelGroup, 1f, 0.3f, useUnscaledTime: true);
        }

        if (menuLeftPanel != null)
        {
            Tween.UIAnchoredPositionX(menuLeftPanel, 0f, 0.4f, Ease.OutBack, useUnscaledTime: true);
        }
        
        if (OverworldHUDManager.Instance != null)
        {
            OverworldHUDManager.Instance.UpdateHUDData();
        }
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        PlayerMovement.IsMenuOpen = false;
        Time.timeScale = 1f;

        Tween.Custom(AudioListener.volume, 1f, 0.3f, onValueChange: v => AudioListener.volume = v, useUnscaledTime: true);

        if (vcamMenu != null) vcamMenu.Priority = 0;

        if (menuLeftPanel != null)
        {
            Tween.UIAnchoredPositionX(menuLeftPanel, panelSlideOffset, 0.3f, Ease.InBack, useUnscaledTime: true);
        }
        if (menuPanelGroup != null)
        {
            Tween.Alpha(menuPanelGroup, 0f, 0.3f, useUnscaledTime: true).OnComplete(() => menuPanelGroup.gameObject.SetActive(false));
        }
    }

    public void Btn_OpenSkills() => OpenSubMenu(skillsPanel);
    public void Btn_OpenInventory() => OpenSubMenu(inventoryPanel);
    public void Btn_OpenRelations() => OpenSubMenu(relationsPanel);
    public void Btn_OpenQuests() => OpenSubMenu(questsPanel);
    public void Btn_OpenConfig() => OpenSubMenu(configPanel);

    private void OpenSubMenu(CanvasGroup subPanel)
    {
        if (subPanel == null) return;

        activeSubPanel = subPanel;
        Time.timeScale = 0f;

        if (menuLeftPanel != null)
        {
            Tween.UIAnchoredPositionX(menuLeftPanel, panelSlideOffset, 0.3f, Ease.InBack, useUnscaledTime: true);
        }

        subPanel.gameObject.SetActive(true);
        subPanel.alpha = 0f;
        Tween.Alpha(subPanel, 1f, 0.3f, useUnscaledTime: true);
    }

    private void CloseSubMenu()
    {
        Time.timeScale = 1f;

        if (activeSubPanel != null)
        {
            CanvasGroup panelToClose = activeSubPanel;
            Tween.Alpha(panelToClose, 0f, 0.3f, useUnscaledTime: true).OnComplete(() => 
            {
                panelToClose.gameObject.SetActive(false);
            });
            activeSubPanel = null;
        }

        if (menuLeftPanel != null)
        {
            Tween.UIAnchoredPositionX(menuLeftPanel, 0f, 0.4f, Ease.OutBack, useUnscaledTime: true);
        }
    }

    private void HideAllSubPanelsFast()
    {
        CanvasGroup[] allPanels = { skillsPanel, inventoryPanel, relationsPanel, questsPanel, configPanel };
        foreach (var p in allPanels)
        {
            if (p != null)
            {
                p.alpha = 0f;
                p.gameObject.SetActive(false);
            }
        }
    }
}