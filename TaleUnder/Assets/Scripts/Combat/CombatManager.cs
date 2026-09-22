using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PrimeTween;
using System.Collections.Generic;
using Unity.Cinemachine;
using TMPro;

public enum CombatState { ActionMenu, SkillSelection, ItemSelection, TargetSelection, AttackMinigame, DefenseGrid, Victory, Defeat }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public static EncounterSO PendingEncounter;

    public CombatState currentState = CombatState.ActionMenu;
    public EncounterSO currentEncounter;
    public PlayerProfileSO playerProfile;

    [Header("UI Status & Info")]
    public Slider playerHPSlider;
    public TMP_Text playerHPText;
    public Slider playerPPSlider;
    public TMP_Text playerPPText;
    public TMP_Text descriptionText; 

    [Header("Arena Setup")]
    public Transform environmentHolder;
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints = new Transform[0]; 

    [Header("Cinematography")]
    public CinemachineCamera vcamCombatMain;
    public CinemachineCamera vcamSkillMenu;
    public CinemachineCamera vcamItemMenu;
    public CinemachineCamera vcamDefense;
    public CinemachineCamera vcamTargetFocus;
    public CinemachineCamera vcamAttackFocus;
    public CinemachineTargetGroup attackTargetGroup;

    [Header("Action Menu")]
    public CanvasGroup actionMenuCanvasGroup;
    public GameObject defenseGridContainer;
    public Image[] actionButtons = new Image[0];
    public Color selectedActionColor = Color.yellow;
    public Color defaultActionColor = Color.white;
    public PlayerSkillSO basicAttackSkill;

    [Header("Skill Menu")]
    public CanvasGroup skillMenuCanvasGroup;
    public Image[] skillButtons = new Image[0];
    
    [Header("Item Menu")]
    public CanvasGroup itemMenuCanvasGroup;
    public Image[] itemButtons = new Image[0];

    [Header("Game Over / Victory")]
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;
    public Image[] gameOverButtons = new Image[0];
    public GameObject victoryPanel;
    public CanvasGroup victoryCanvasGroup;
    public Image[] victoryButtons = new Image[0];

    [Header("Combat Flow")]
    public GameObject normalMetronomeContainer;
    public CanvasGroup combatDimmer;
    public int warmupBeats = 2;

    [Header("Grid References")]
    public CombatGridPlayer gridPlayer;
    public CombatGridVisuals gridVisuals;

    [Header("Testing & Audio")]
    public EnemyActionPatternSO testPattern;
    public AudioSource sfxSource;
    public AudioClip damageSFX;
    public AudioClip warmupSFX;
    public AudioClip stepSFX;

    private int selectedActionIndex = 0;
    private int selectedSkillIndex = 0;
    private int selectedItemIndex = 0;
    private int gameOverSelectedIndex = 0;
    private int victorySelectedIndex = 0;
    
    private int selectedTargetIndex = 0;
    private bool isTargetingAlly = false;
    
    private PlayerSkillSO activeSkillSO;
    private ItemSO activeItemSO;
    private ICombatMinigame activeMinigameInstance;
    
    private bool isDefending = false;
    private bool isPatternActive = false;
    private int currentPatternBeat = 0;
    
    private bool isWaitingForAttackSync = false;
    private int warmupCounter = 0;
    private int attackMinigameBeat = 0;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<EnemyCombatUI> enemyUIList = new List<EnemyCombatUI>();
    
    private bool isRunningAway = false;
    private bool tookDamageDuringRun = false;
    private Transform playerRootTransform;
    private Transform playerVisualTransform;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (PendingEncounter != null) 
        { 
            currentEncounter = PendingEncounter; 
            PendingEncounter = null; 
        }
    }

    public void Start()
    {
        PlayerMovement.IsInCombat = true;
        AudioListener.volume = 1f;
        InitializeEncounter();

        if (currentEncounter != null && RhythmManager.Instance != null)
        {
            RhythmManager.Instance.PlayTrack(currentEncounter.battleMusic);
            RhythmManager.Instance.OnBeat += HandleBeat;
        }

        EnterActionMenu();
    }

    public void OnDestroy()
    {
        PlayerMovement.IsInCombat = false;
        if (RhythmManager.Instance != null) 
        {
            RhythmManager.Instance.OnBeat -= HandleBeat;
        }
    }

    public void Update()
    {
        if (currentState == CombatState.ActionMenu) HandleActionMenuInput();
        else if (currentState == CombatState.SkillSelection) HandleSkillSelectionInput();
        else if (currentState == CombatState.ItemSelection) HandleItemSelectionInput();
        else if (currentState == CombatState.TargetSelection) HandleTargetSelectionInput();
        else if (currentState == CombatState.DefenseGrid && gridPlayer != null) gridPlayer.ProcessInput();
        else if (currentState == CombatState.Defeat) HandleGameOverInput();
        else if (currentState == CombatState.Victory) HandleVictoryInput();
    }

    private void UpdatePlayerStatsUI()
    {
        if (playerProfile == null) return;
        if (playerHPSlider != null) 
        { 
            playerHPSlider.maxValue = playerProfile.GetTotalMaxHP(); 
            Tween.UISliderValue(playerHPSlider, playerProfile.currentHP, 0.3f, Ease.OutBounce); 
        }
        if (playerPPSlider != null) 
        { 
            playerPPSlider.maxValue = playerProfile.maxPP; 
            Tween.UISliderValue(playerPPSlider, playerProfile.currentPP, 0.3f, Ease.OutBounce); 
        }
        if (playerHPText != null) playerHPText.text = $"{playerProfile.currentHP}/{playerProfile.GetTotalMaxHP()}";
        if (playerPPText != null) playerPPText.text = $"{playerProfile.currentPP}/{playerProfile.maxPP}";
    }

    private void InitializeEncounter()
    {
        activeEnemies.Clear();
        enemyUIList.Clear();

        GameObject playerRoot = GameObject.FindGameObjectWithTag("Player");
        if (playerRoot != null && playerSpawnPoint != null)
        {
            playerRootTransform = playerRoot.transform;
            playerRootTransform.position = playerSpawnPoint.position;
            playerRootTransform.rotation = playerSpawnPoint.rotation;
            playerVisualTransform = playerRootTransform.Find("PlayerVisual");
            if (playerVisualTransform == null) playerVisualTransform = playerRootTransform;
        }

        UpdatePlayerStatsUI();

        if (currentEncounter == null) return;
        if (currentEncounter.combatEnvironmentPrefab != null) 
        {
            Instantiate(currentEncounter.combatEnvironmentPrefab, environmentHolder.position, environmentHolder.rotation, environmentHolder);
        }

        if (currentEncounter.enemiesInEncounter != null && enemySpawnPoints != null)
        {
            int limit = Mathf.Min(currentEncounter.enemiesInEncounter.Length, enemySpawnPoints.Length);
            for (int i = 0; i < limit; i++)
            {
                EnemySO enemyData = currentEncounter.enemiesInEncounter[i];
                if (enemyData != null && enemyData.enemyPrefab != null)
                {
                    GameObject spawnedEnemy = Instantiate(enemyData.enemyPrefab, enemySpawnPoints[i].position, enemySpawnPoints[i].rotation);
                    activeEnemies.Add(spawnedEnemy);
                    EnemyCombatUI uiComp = spawnedEnemy.GetComponentInChildren<EnemyCombatUI>(true);
                    if (uiComp != null) 
                    { 
                        uiComp.Initialize(enemyData.maxHP); 
                        enemyUIList.Add(uiComp); 
                    }
                }
            }
        }
    }

    private void SwitchCamera(CinemachineCamera targetCam)
    {
        if (vcamCombatMain != null) vcamCombatMain.Priority = 0;
        if (vcamSkillMenu != null) vcamSkillMenu.Priority = 0;
        if (vcamItemMenu != null) vcamItemMenu.Priority = 0;
        if (vcamDefense != null) vcamDefense.Priority = 0;
        if (vcamTargetFocus != null) vcamTargetFocus.Priority = 0;
        if (vcamAttackFocus != null) vcamAttackFocus.Priority = 0;
        
        if (targetCam != null) targetCam.Priority = 20;
    }

    private void ShowUIGroup(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.interactable = true;
        group.blocksRaycasts = true;
        Tween.Alpha(group, 1f, 0.2f);
    }

    private void HideUIGroup(CanvasGroup group)
    {
        group.interactable = false;
        group.blocksRaycasts = false;
        Tween.Alpha(group, 0f, 0.2f).OnComplete(() => group.gameObject.SetActive(false));
    }

    public void EnterActionMenu()
    {
        currentState = CombatState.ActionMenu;
        SwitchCamera(vcamCombatMain);

        if (defenseGridContainer != null) defenseGridContainer.SetActive(false);
        if (skillMenuCanvasGroup != null && skillMenuCanvasGroup.gameObject.activeSelf) HideUIGroup(skillMenuCanvasGroup);
        if (itemMenuCanvasGroup != null && itemMenuCanvasGroup.gameObject.activeSelf) HideUIGroup(itemMenuCanvasGroup);
        if (actionMenuCanvasGroup != null) ShowUIGroup(actionMenuCanvasGroup);
        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(true);
        if (descriptionText != null) descriptionText.text = "";

        if (combatDimmer != null) Tween.Alpha(combatDimmer, 0f, 0.3f).OnComplete(() => combatDimmer.gameObject.SetActive(false));

        UpdateActionMenuVisuals();
    }

    private void HandleActionMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedActionIndex = Mathf.Clamp(selectedActionIndex + 1, 0, 4);
            UpdateActionMenuVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedActionIndex = Mathf.Clamp(selectedActionIndex - 1, 0, 4);
            UpdateActionMenuVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteAction(selectedActionIndex);
        }
    }

    private void UpdateActionMenuVisuals()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null) 
            {
                actionButtons[i].color = (i == selectedActionIndex) ? selectedActionColor : defaultActionColor;
            }
        }
    }

    private void ExecuteAction(int actionIndex)
    {
        activeSkillSO = null;
        activeItemSO = null;
        isTargetingAlly = false;

        switch (actionIndex)
        {
            case 0:
                if (playerProfile != null) 
                { 
                    activeSkillSO = playerProfile.basicAttack; 
                    if (activeSkillSO != null) isTargetingAlly = activeSkillSO.targetsAlly; 
                }
                StartTargetSelection(); 
                break;
            case 1: 
                StartSkillSelection(); 
                break;
            case 2: 
                StartItemSelection(); 
                break;
            case 3: 
                StartDefensePhase(); 
                break;
            case 4: 
                isRunningAway = true;
                tookDamageDuringRun = false;
                if (playerVisualTransform != null) 
                {
                    Tween.LocalRotation(playerVisualTransform, Quaternion.Euler(0, 360, 0), 0.5f, Ease.InOutSine, cycles: 2);
                }
                StartDefensePhase();
                break;
        }
    }

    private void StartSkillSelection()
    {
        if (playerProfile == null || playerProfile.equippedSkills == null || playerProfile.equippedSkills.Count == 0) return;
        currentState = CombatState.SkillSelection;
        selectedSkillIndex = 0;
        
        SwitchCamera(vcamSkillMenu != null ? vcamSkillMenu : vcamCombatMain);
        if (actionMenuCanvasGroup != null) HideUIGroup(actionMenuCanvasGroup);
        if (skillMenuCanvasGroup != null) ShowUIGroup(skillMenuCanvasGroup);
        
        UpdateSkillMenuVisuals();
    }

    private void HandleSkillSelectionInput()
    {
        int maxIndex = playerProfile.equippedSkills.Count - 1;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) 
        { 
            selectedSkillIndex = Mathf.Clamp(selectedSkillIndex + 1, 0, maxIndex); 
            UpdateSkillMenuVisuals(); 
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) 
        { 
            selectedSkillIndex = Mathf.Clamp(selectedSkillIndex - 1, 0, maxIndex); 
            UpdateSkillMenuVisuals(); 
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            PlayerSkillSO chosenSkill = playerProfile.equippedSkills[selectedSkillIndex];
            if (playerProfile.currentPP >= chosenSkill.ppCost)
            {
                activeSkillSO = chosenSkill;
                isTargetingAlly = chosenSkill.targetsAlly;
                StartTargetSelection();
            }
            else 
            {
                PlaySFX(damageSFX);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) 
        {
            EnterActionMenu();
        }
    }

    private void UpdateSkillMenuVisuals()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] != null) 
            {
                skillButtons[i].color = (i == selectedSkillIndex) ? selectedActionColor : defaultActionColor;
            }
        }
        if (descriptionText != null && playerProfile.equippedSkills.Count > selectedSkillIndex)
        {
            var skill = playerProfile.equippedSkills[selectedSkillIndex];
            descriptionText.text = $"<b>{skill.skillName}</b>\nCost: {skill.ppCost} PP\n{skill.description}";
        }
    }

    private void StartItemSelection()
    {
        if (playerProfile == null || playerProfile.inventory == null || playerProfile.inventory.Count == 0) return;
        currentState = CombatState.ItemSelection;
        selectedItemIndex = 0;
        
        SwitchCamera(vcamItemMenu != null ? vcamItemMenu : vcamCombatMain);
        if (actionMenuCanvasGroup != null) HideUIGroup(actionMenuCanvasGroup);
        if (itemMenuCanvasGroup != null) ShowUIGroup(itemMenuCanvasGroup);
        
        UpdateItemMenuVisuals();
    }

    private void HandleItemSelectionInput()
    {
        int maxIndex = playerProfile.inventory.Count - 1;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) 
        { 
            selectedItemIndex = Mathf.Clamp(selectedItemIndex + 1, 0, maxIndex); 
            UpdateItemMenuVisuals(); 
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) 
        { 
            selectedItemIndex = Mathf.Clamp(selectedItemIndex - 1, 0, maxIndex); 
            UpdateItemMenuVisuals(); 
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            ItemSO chosenItem = playerProfile.inventory[selectedItemIndex].item;
            if (chosenItem == null) return;
            
            activeItemSO = chosenItem;
            isTargetingAlly = chosenItem.targetsAlly;
            StartTargetSelection();
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) 
        {
            EnterActionMenu();
        }
    }

    private void UpdateItemMenuVisuals()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null) 
            {
                itemButtons[i].color = (i == selectedItemIndex) ? selectedActionColor : defaultActionColor;
            }
        }
        if (descriptionText != null && playerProfile.inventory.Count > selectedItemIndex)
        {
            var slot = playerProfile.inventory[selectedItemIndex];
            descriptionText.text = $"<b>{slot.item.itemName}</b> (x{slot.amount})\n{slot.item.description}";
        }
    }

    private void StartTargetSelection()
    {
        if (activeEnemies.Count == 0) return;
        currentState = CombatState.TargetSelection;
        
        if (skillMenuCanvasGroup != null) HideUIGroup(skillMenuCanvasGroup);
        if (itemMenuCanvasGroup != null) HideUIGroup(itemMenuCanvasGroup);
        if (descriptionText != null) descriptionText.text = isTargetingAlly ? "Select Ally" : "Select Target";
        
        selectedTargetIndex = isTargetingAlly ? 0 : GetNextAliveEnemyIndex(0);
        
        if (!isTargetingAlly && selectedTargetIndex == -1) 
        { 
            TriggerVictory(); 
            return; 
        }
        HighlightTarget();
    }

    private void HandleTargetSelectionInput()
    {
        if (!isTargetingAlly)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) 
            { 
                selectedTargetIndex = GetNextAliveEnemyIndex((selectedTargetIndex + 1) % activeEnemies.Count); 
                HighlightTarget(); 
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) 
            { 
                selectedTargetIndex = GetNextAliveEnemyIndex((selectedTargetIndex - 1 + activeEnemies.Count) % activeEnemies.Count); 
                HighlightTarget(); 
            }
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            if (activeItemSO != null) 
            {
                ExecuteItemEffect(isTargetingAlly ? playerRootTransform : activeEnemies[selectedTargetIndex].transform);
            }
            else 
            {
                StartAttackMinigame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            ClearHighlights();
            EnterActionMenu();
        }
    }

    private void HighlightTarget()
    {
        Transform focusTransform = null;
        if (isTargetingAlly)
        {
            focusTransform = playerRootTransform;
            if (playerRootTransform != null)
            {
                Tween.Scale(playerRootTransform, Vector3.one * 1.2f, 0.2f);
            }
        }
        else
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null && enemyUIList[i] != null)
                {
                    if (i == selectedTargetIndex) 
                    { 
                        Tween.Scale(activeEnemies[i].transform, Vector3.one * 1.2f, 0.2f); 
                        enemyUIList[i].ShowUI(); 
                        focusTransform = activeEnemies[i].transform; 
                    }
                    else 
                    { 
                        Tween.Scale(activeEnemies[i].transform, Vector3.one, 0.2f); 
                        enemyUIList[i].HideUI(); 
                    }
                }
            }
        }

        SwitchCamera(vcamTargetFocus);
        if (vcamTargetFocus != null && focusTransform != null)
        {
            vcamTargetFocus.Follow = focusTransform;
            vcamTargetFocus.LookAt = focusTransform;
        }
    }

    private void ClearHighlights()
    {
        if (playerRootTransform != null) 
        {
            Tween.Scale(playerRootTransform, Vector3.one, 0.2f);
        }
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null) 
            { 
                Tween.Scale(activeEnemies[i].transform, Vector3.one, 0.2f); 
                if (enemyUIList[i] != null) enemyUIList[i].HideUI(); 
            }
        }
    }

    private void ExecuteItemEffect(Transform target)
    {
        if (activeItemSO == null || playerProfile == null) return;
        if (activeItemSO.useSFX != null) PlaySFX(activeItemSO.useSFX);

        if (activeItemSO.effectType == ItemEffectType.HealHP)
        {
            playerProfile.currentHP = Mathf.Clamp(playerProfile.currentHP + activeItemSO.effectValue, 0, playerProfile.GetTotalMaxHP());
            if (activeItemSO.vfxPrefab != null && target != null) 
            {
                Instantiate(activeItemSO.vfxPrefab, target.position, Quaternion.identity);
            }
            Tween.Delay(1f).OnComplete(StartDefensePhase);
        }
        else if (activeItemSO.effectType == ItemEffectType.HealPP)
        {
            playerProfile.currentPP = Mathf.Clamp(playerProfile.currentPP + activeItemSO.effectValue, 0, playerProfile.maxPP);
            if (activeItemSO.vfxPrefab != null && target != null) 
            {
                Instantiate(activeItemSO.vfxPrefab, target.position, Quaternion.identity);
            }
            Tween.Delay(1f).OnComplete(StartDefensePhase);
        }
        else if (activeItemSO.effectType == ItemEffectType.DamageEnemy && target != null && selectedTargetIndex >= 0)
        {
            EnemyCombatUI targetUI = enemyUIList[selectedTargetIndex];
            int newHP = targetUI.GetCurrentHP() - activeItemSO.effectValue;
            targetUI.UpdateHP(newHP);
            if (activeItemSO.vfxPrefab != null) 
            {
                Instantiate(activeItemSO.vfxPrefab, target.position, Quaternion.identity);
            }
            Tween.ShakeLocalPosition(target, strength: new Vector3(0.5f, 0f, 0f), duration: 0.2f);

            if (newHP <= 0)
            {
                Tween.Scale(target, Vector3.zero, 0.5f).OnComplete(() => activeEnemies[selectedTargetIndex].SetActive(false));
                Tween.Delay(1f).OnComplete(() => 
                { 
                    if (GetNextAliveEnemyIndex(0) == -1) TriggerVictory(); 
                    else StartDefensePhase(); 
                });
            }
            else 
            {
                Tween.Delay(1f).OnComplete(StartDefensePhase);
            }
        }

        UpdatePlayerStatsUI();
        playerProfile.ConsumeItem(activeItemSO);
        ClearHighlights();
    }

    private void StartAttackMinigame()
    {
        currentState = CombatState.AttackMinigame;
        isWaitingForAttackSync = true;
        warmupCounter = 0;

        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(false);
        if (combatDimmer != null) 
        { 
            combatDimmer.gameObject.SetActive(true); 
            Tween.Alpha(combatDimmer, 1f, 0.3f); 
        }

        float targetAlpha = 0f;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null && (!isTargetingAlly && i != selectedTargetIndex))
            {
                SpriteRenderer sr = activeEnemies[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null) Tween.Color(sr, new Color(1f, 1f, 1f, targetAlpha), 0.3f);
            }
        }

        if (attackTargetGroup != null && playerRootTransform != null)
        {
            attackTargetGroup.Targets.Clear();
            attackTargetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = playerRootTransform, Weight = 1f, Radius = 2f });
            if (!isTargetingAlly && activeEnemies.Count > selectedTargetIndex && activeEnemies[selectedTargetIndex] != null)
            {
                attackTargetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = activeEnemies[selectedTargetIndex].transform, Weight = 1f, Radius = 2f });
            }
        }

        SwitchCamera(vcamAttackFocus);

        if (activeSkillSO != null && activeSkillSO.minigamePrefab != null)
        {
            GameObject spawnedMinigame = Instantiate(activeSkillSO.minigamePrefab);
            activeMinigameInstance = spawnedMinigame.GetComponent<ICombatMinigame>();
            playerProfile.currentPP = Mathf.Clamp(playerProfile.currentPP - activeSkillSO.ppCost, 0, playerProfile.maxPP);
            UpdatePlayerStatsUI();
        }
        else 
        {
            OnAttackMinigameEnded(0f);
        }
    }

    private void HandleBeat(int absoluteBeat, int measureBeat)
    {
        if (currentState == CombatState.AttackMinigame)
        {
            if (isWaitingForAttackSync)
            {
                warmupCounter++;
                if (warmupCounter >= warmupBeats)
                {
                    isWaitingForAttackSync = false;
                    attackMinigameBeat = 1;
                    
                    if (activeMinigameInstance != null)
                    {
                        activeMinigameInstance.OnMinigameEnd = OnAttackMinigameEnded;
                        Transform target = isTargetingAlly ? playerRootTransform : activeEnemies[selectedTargetIndex].transform;
                        activeMinigameInstance.StartMinigame(target, activeSkillSO);
                        activeMinigameInstance.ExecuteBeat(attackMinigameBeat);
                    }
                }
                else 
                {
                    PlaySFX(warmupSFX);
                }
            }
            else
            {
                attackMinigameBeat++;
                if (activeMinigameInstance != null) activeMinigameInstance.ExecuteBeat(attackMinigameBeat);
                if (attackMinigameBeat < 3) PlaySFX(stepSFX);
            }
        }
        else if (currentState == CombatState.DefenseGrid && isDefending)
        {
            if (!isPatternActive && measureBeat == 1) isPatternActive = true;
            if (isPatternActive)
            {
                currentPatternBeat++;
                if (testPattern != null) testPattern.ExecuteBeat(currentPatternBeat, this);
            }
        }
    }

    private void OnAttackMinigameEnded(float damageMultiplier)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                SpriteRenderer sr = activeEnemies[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null) Tween.Color(sr, new Color(1f, 1f, 1f, 1f), 0.3f);
            }
        }

        if (damageMultiplier > 0f && playerProfile != null && !isTargetingAlly)
        {
            int finalDamage = Mathf.RoundToInt(playerProfile.GetTotalAttack() * damageMultiplier);
            if (selectedTargetIndex >= 0 && selectedTargetIndex < enemyUIList.Count)
            {
                EnemyCombatUI targetUI = enemyUIList[selectedTargetIndex];
                int newHP = targetUI.GetCurrentHP() - finalDamage;
                targetUI.UpdateHP(newHP);
                Tween.ShakeLocalPosition(activeEnemies[selectedTargetIndex].transform, strength: new Vector3(0.5f, 0f, 0f), duration: 0.2f);
                PlaySFX(damageSFX);

                if (newHP <= 0) 
                {
                    Tween.Scale(activeEnemies[selectedTargetIndex].transform, Vector3.zero, 0.5f).OnComplete(() => activeEnemies[selectedTargetIndex].SetActive(false));
                }
            }
        }
        
        ClearHighlights();
        if (GetNextAliveEnemyIndex(0) == -1) TriggerVictory();
        else StartDefensePhase();
    }

    private void StartDefensePhase()
    {
        currentState = CombatState.DefenseGrid;
        isDefending = true;
        isPatternActive = false;
        currentPatternBeat = 0;

        SwitchCamera(vcamDefense != null ? vcamDefense : vcamCombatMain);

        if (actionMenuCanvasGroup != null) HideUIGroup(actionMenuCanvasGroup);
        if (defenseGridContainer != null) defenseGridContainer.SetActive(true);
        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(false);

        if (combatDimmer != null && !combatDimmer.gameObject.activeSelf) 
        { 
            combatDimmer.gameObject.SetActive(true); 
            Tween.Alpha(combatDimmer, 1f, 0.3f); 
        }
        Canvas.ForceUpdateCanvases();
        
        if (gridPlayer != null) 
        { 
            gridPlayer.currentGridPos = new Vector2Int(1, 1); 
            gridPlayer.SnapToPosition(); 
        }
    }

    public void EndDefensePhase()
    {
        isDefending = false;
        isPatternActive = false;

        if (isRunningAway)
        {
            isRunningAway = false;
            if (!tookDamageDuringRun) 
            { 
                ExecuteVictoryAction(true); 
                return; 
            }
        }
        
        EnterActionMenu();
    }

    public void CheckPlayerHit(int gridX, int gridY, int damage)
    {
        if (gridPlayer.currentGridPos.x == gridX && gridPlayer.currentGridPos.y == gridY)
        {
            tookDamageDuringRun = true;
            PlaySFX(damageSFX);
            
            if (playerProfile != null)
            {
                playerProfile.currentHP = Mathf.Clamp(playerProfile.currentHP - damage, 0, playerProfile.GetTotalMaxHP());
                UpdatePlayerStatsUI();
                Tween.ShakeLocalPosition(gridPlayer.transform, strength: new Vector3(25f, 25f, 0f), duration: 0.3f);
                
                if (playerProfile.currentHP <= 0 && currentState != CombatState.Defeat) TriggerGameOver();
            }
        }
    }

    private int GetNextAliveEnemyIndex(int startIndex)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            int checkIndex = (startIndex + i) % activeEnemies.Count;
            if (activeEnemies[checkIndex] != null && enemyUIList[checkIndex].GetCurrentHP() > 0) return checkIndex;
        }
        return -1;
    }

    private void TriggerGameOver()
    {
        currentState = CombatState.Defeat;
        isDefending = false;
        gameOverSelectedIndex = 0;
        
        Tween.Custom(1f, 0f, 2f, onValueChange: v => AudioListener.volume = v);
        
        if (gameOverPanel != null && gameOverCanvasGroup != null)
        {
            gameOverPanel.SetActive(true);
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
            Tween.Alpha(gameOverCanvasGroup, 1f, 2f);
            UpdateGameOverVisuals();
        }
    }

    private void TriggerVictory()
    {
        currentState = CombatState.Victory;
        isDefending = false;
        victorySelectedIndex = 0;
        
        Tween.Custom(1f, 0f, 2f, onValueChange: v => AudioListener.volume = v);
        
        if (victoryPanel != null && victoryCanvasGroup != null)
        {
            victoryPanel.SetActive(true);
            victoryCanvasGroup.alpha = 0f;
            victoryCanvasGroup.interactable = true;
            victoryCanvasGroup.blocksRaycasts = true;
            Tween.Alpha(victoryCanvasGroup, 1f, 2f);
            UpdateVictoryVisuals();
        }
    }

    private void HandleGameOverInput()
    {
        int maxIndex = (gameOverButtons != null && gameOverButtons.Length > 0) ? gameOverButtons.Length - 1 : 0;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            gameOverSelectedIndex = Mathf.Clamp(gameOverSelectedIndex + 1, 0, maxIndex);
            UpdateGameOverVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            gameOverSelectedIndex = Mathf.Clamp(gameOverSelectedIndex - 1, 0, maxIndex);
            UpdateGameOverVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)) 
        {
            ExecuteGameOverAction(gameOverSelectedIndex);
        }
    }

    private void HandleVictoryInput()
    {
        int maxIndex = (victoryButtons != null && victoryButtons.Length > 0) ? victoryButtons.Length - 1 : 0;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            victorySelectedIndex = Mathf.Clamp(victorySelectedIndex + 1, 0, maxIndex);
            UpdateVictoryVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            victorySelectedIndex = Mathf.Clamp(victorySelectedIndex - 1, 0, maxIndex);
            UpdateVictoryVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)) 
        {
            ExecuteVictoryAction(false);
        }
    }

    private void UpdateGameOverVisuals()
    {
        if (gameOverButtons == null) return;
        for (int i = 0; i < gameOverButtons.Length; i++)
        {
            if (gameOverButtons[i] != null) 
            {
                gameOverButtons[i].color = (i == gameOverSelectedIndex) ? selectedActionColor : defaultActionColor;
            }
        }
    }

    private void UpdateVictoryVisuals()
    {
        if (victoryButtons == null) return;
        for (int i = 0; i < victoryButtons.Length; i++)
        {
            if (victoryButtons[i] != null) 
            {
                victoryButtons[i].color = (i == victorySelectedIndex) ? selectedActionColor : defaultActionColor;
            }
        }
    }

    private void ExecuteGameOverAction(int index)
    {
        switch (index)
        {
            case 0: RestartBattle(); break;
            case 1: LoadGame(); break;
            case 2: ReturnToMainMenu(); break;
        }
    }

    public void ExecuteVictoryAction(bool fled = false)
    {
        if (RhythmManager.Instance != null) 
        {
            RhythmManager.Instance.StopTrack();
        }
        
        CombatStatePayload.IsReturningFromCombat = true;
        CombatStatePayload.FledCombat = fled;
        CombatStatePayload.WonCombat = !fled;
        AudioListener.volume = 1f;
        
        SceneManager.LoadScene(CombatStatePayload.ReturnSceneName);
    }

    public void RestartBattle()
    {
        PendingEncounter = currentEncounter;
        if (playerProfile != null) 
        {
            playerProfile.currentHP = playerProfile.GetTotalMaxHP();
        }
        
        if (RhythmManager.Instance != null) 
        {
            RhythmManager.Instance.StopTrack();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadGame() 
    { 
        
    }

    public void ReturnToMainMenu() 
    { 
        SceneManager.LoadScene("MainMenu"); 
    }

    public void PlaySFX(AudioClip clip) 
    { 
        if (clip != null && sfxSource != null) 
        {
            sfxSource.PlayOneShot(clip); 
        }
    }
}