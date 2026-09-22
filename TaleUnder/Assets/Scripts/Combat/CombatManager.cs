using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PrimeTween;
using System.Collections.Generic;
using Unity.Cinemachine;

public enum CombatState { ActionMenu, SkillSelection, ItemSelection, TargetSelection, AttackMinigame, DefenseGrid, Victory, Defeat }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public static EncounterSO PendingEncounter;

    public CombatState currentState = CombatState.ActionMenu;
    public EncounterSO currentEncounter;

    [Header("Player Data")]
    public PlayerProfileSO playerProfile;

    [Header("UI Status")]
    public Slider playerHPSlider;
    public Slider playerPPSlider;

    [Header("Arena Setup")]
    public Transform environmentHolder;
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints; 

    [Header("Camera References")]
    public CinemachineCamera vcamCombatMain;
    public CinemachineCamera vcamTargetFocus;
    public CinemachineCamera vcamAttackFocus;
    public CinemachineTargetGroup attackTargetGroup;

    [Header("Action Menu")]
    public RectTransform actionMenuContainer;
    public GameObject defenseGridContainer;
    public Image[] actionButtons;
    public Color selectedActionColor = Color.yellow;
    public Color defaultActionColor = Color.white;

    [Header("Skill Menu")]
    public RectTransform skillMenuContainer;
    public Image[] skillButtons;
    
    [Header("Item Menu")]
    public RectTransform itemMenuContainer;
    public Image[] itemButtons;

    [Header("Game Over / Victory")]
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;
    public Image[] gameOverButtons;
    public GameObject victoryPanel;
    public CanvasGroup victoryCanvasGroup;
    public Image[] victoryButtons;

    [Header("Combat Flow")]
    public GameObject normalMetronomeContainer;
    public CanvasGroup combatDimmer;
    public int warmupBeats = 2;

    [Header("Grid References")]
    public CombatGridPlayer gridPlayer;
    public CombatGridVisuals gridVisuals;

    [Header("Testing")]
    public EnemyActionPatternSO testPattern;

    [Header("Audio")]
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
        
        if (vcamCombatMain != null) vcamCombatMain.Priority = 10;
        if (vcamTargetFocus != null) vcamTargetFocus.Priority = 0;
        if (vcamAttackFocus != null) vcamAttackFocus.Priority = 0;

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
        if (RhythmManager.Instance != null) RhythmManager.Instance.OnBeat -= HandleBeat;
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
        }

        if (playerProfile != null)
        {
            if (playerHPSlider != null) { playerHPSlider.maxValue = playerProfile.GetTotalMaxHP(); playerHPSlider.value = playerProfile.currentHP; }
            if (playerPPSlider != null) { playerPPSlider.maxValue = playerProfile.maxPP; playerPPSlider.value = playerProfile.currentPP; }
        }

        if (currentEncounter == null) return;

        if (currentEncounter.combatEnvironmentPrefab != null) Instantiate(currentEncounter.combatEnvironmentPrefab, environmentHolder.position, environmentHolder.rotation, environmentHolder);

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

    public void EnterActionMenu()
    {
        currentState = CombatState.ActionMenu;

        if (vcamCombatMain != null) vcamCombatMain.Priority = 10;
        if (vcamTargetFocus != null) vcamTargetFocus.Priority = 0;
        if (vcamAttackFocus != null) vcamAttackFocus.Priority = 0;

        if (defenseGridContainer != null) defenseGridContainer.SetActive(false);
        if (skillMenuContainer != null) skillMenuContainer.gameObject.SetActive(false);
        if (itemMenuContainer != null) itemMenuContainer.gameObject.SetActive(false);
        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(true);
        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(true);

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
            if (actionButtons[i] != null) actionButtons[i].color = (i == selectedActionIndex) ? selectedActionColor : defaultActionColor;
        }
    }

    private void ExecuteAction(int actionIndex)
    {
        activeSkillSO = null;
        activeItemSO = null;

        switch (actionIndex)
        {
            case 0:
                if (playerProfile != null) activeSkillSO = playerProfile.basicAttack;
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
                StartDefensePhase();
                break;
        }
    }

    private void StartSkillSelection()
    {
        if (playerProfile == null || playerProfile.equippedSkills == null || playerProfile.equippedSkills.Count == 0) return;

        currentState = CombatState.SkillSelection;
        selectedSkillIndex = 0;
        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(false);
        if (skillMenuContainer != null) skillMenuContainer.gameObject.SetActive(true);
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
                StartTargetSelection();
            }
            else PlaySFX(damageSFX);
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) EnterActionMenu();
    }

    private void UpdateSkillMenuVisuals()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] != null) skillButtons[i].color = (i == selectedSkillIndex) ? selectedActionColor : defaultActionColor;
        }
    }

    private void StartItemSelection()
    {
        if (playerProfile == null || playerProfile.inventory == null || playerProfile.inventory.Count == 0) return;

        currentState = CombatState.ItemSelection;
        selectedItemIndex = 0;
        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(false);
        if (itemMenuContainer != null) itemMenuContainer.gameObject.SetActive(true);
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
            
            if (activeItemSO.effectType == ItemEffectType.DamageEnemy)
            {
                StartTargetSelection();
            }
            else
            {
                ExecuteItemEffect(null);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) EnterActionMenu();
    }

    private void UpdateItemMenuVisuals()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null) itemButtons[i].color = (i == selectedItemIndex) ? selectedActionColor : defaultActionColor;
        }
    }

    private void ExecuteItemEffect(Transform enemyTarget)
    {
        if (activeItemSO == null || playerProfile == null) return;

        if (activeItemSO.useSFX != null) PlaySFX(activeItemSO.useSFX);

        switch (activeItemSO.effectType)
        {
            case ItemEffectType.HealHP:
                playerProfile.currentHP = Mathf.Clamp(playerProfile.currentHP + activeItemSO.effectValue, 0, playerProfile.GetTotalMaxHP());
                if (playerHPSlider != null) Tween.UISliderValue(playerHPSlider, playerProfile.currentHP, 0.3f, Ease.OutBounce);
                if (activeItemSO.vfxPrefab != null && playerRootTransform != null) Instantiate(activeItemSO.vfxPrefab, playerRootTransform.position, Quaternion.identity);
                Tween.Delay(1f).OnComplete(StartDefensePhase);
                break;

            case ItemEffectType.HealPP:
                playerProfile.currentPP = Mathf.Clamp(playerProfile.currentPP + activeItemSO.effectValue, 0, playerProfile.maxPP);
                if (playerPPSlider != null) Tween.UISliderValue(playerPPSlider, playerProfile.currentPP, 0.3f, Ease.OutBounce);
                if (activeItemSO.vfxPrefab != null && playerRootTransform != null) Instantiate(activeItemSO.vfxPrefab, playerRootTransform.position, Quaternion.identity);
                Tween.Delay(1f).OnComplete(StartDefensePhase);
                break;

            case ItemEffectType.DamageEnemy:
                if (enemyTarget != null && selectedTargetIndex >= 0)
                {
                    EnemyCombatUI targetUI = enemyUIList[selectedTargetIndex];
                    int newHP = targetUI.GetCurrentHP() - activeItemSO.effectValue;
                    targetUI.UpdateHP(newHP);

                    if (activeItemSO.vfxPrefab != null) Instantiate(activeItemSO.vfxPrefab, enemyTarget.position, Quaternion.identity);
                    Tween.ShakeLocalPosition(enemyTarget, strength: new Vector3(0.5f, 0f, 0f), duration: 0.2f);

                    if (newHP <= 0)
                    {
                        Tween.Scale(enemyTarget, Vector3.zero, 0.5f).OnComplete(() => activeEnemies[selectedTargetIndex].SetActive(false));
                        Tween.Delay(1f).OnComplete(() => { if (GetNextAliveEnemyIndex(0) == -1) TriggerVictory(); else StartDefensePhase(); });
                    }
                    else
                    {
                        Tween.Delay(1f).OnComplete(StartDefensePhase);
                    }
                }
                break;
        }

        playerProfile.ConsumeItem(activeItemSO);

        if (itemMenuContainer != null) itemMenuContainer.gameObject.SetActive(false);
        ClearHighlights();
    }

    private void StartTargetSelection()
    {
        if (activeEnemies.Count == 0) return;

        currentState = CombatState.TargetSelection;
        if (skillMenuContainer != null) skillMenuContainer.gameObject.SetActive(false);
        if (itemMenuContainer != null) itemMenuContainer.gameObject.SetActive(false);
        
        selectedTargetIndex = GetNextAliveEnemyIndex(0);
        
        if (selectedTargetIndex == -1)
        {
            TriggerVictory();
            return;
        }

        HighlightTarget();
    }

    private void HandleTargetSelectionInput()
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

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            if (activeItemSO != null && activeItemSO.effectType == ItemEffectType.DamageEnemy)
            {
                ExecuteItemEffect(activeEnemies[selectedTargetIndex].transform);
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

    private int GetNextAliveEnemyIndex(int startIndex)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            int checkIndex = (startIndex + i) % activeEnemies.Count;
            if (activeEnemies[checkIndex] != null && enemyUIList[checkIndex].GetCurrentHP() > 0) return checkIndex;
        }
        return -1;
    }

    private void HighlightTarget()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null && enemyUIList[i] != null)
            {
                if (i == selectedTargetIndex)
                {
                    Tween.Scale(activeEnemies[i].transform, Vector3.one * 1.2f, 0.2f);
                    enemyUIList[i].ShowUI();
                    
                    if (vcamTargetFocus != null)
                    {
                        vcamTargetFocus.Follow = activeEnemies[i].transform;
                        vcamTargetFocus.LookAt = activeEnemies[i].transform;
                        vcamTargetFocus.Priority = 20;
                    }
                }
                else
                {
                    Tween.Scale(activeEnemies[i].transform, Vector3.one, 0.2f);
                    enemyUIList[i].HideUI();
                }
            }
        }
    }

    private void ClearHighlights()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                Tween.Scale(activeEnemies[i].transform, Vector3.one, 0.2f);
                if (enemyUIList[i] != null) enemyUIList[i].HideUI();
            }
        }
        if (vcamTargetFocus != null) vcamTargetFocus.Priority = 0;
    }

    private void IsolateForAttack(bool isolate)
    {
        float targetAlpha = isolate ? 0f : 1f;
        float duration = 0.3f;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null && i != selectedTargetIndex)
            {
                SpriteRenderer sr = activeEnemies[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null) Tween.Color(sr, new Color(1f, 1f, 1f, targetAlpha), duration);
            }
        }
    }

    private void StartAttackMinigame()
    {
        currentState = CombatState.AttackMinigame;
        isWaitingForAttackSync = true;
        warmupCounter = 0;

        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(false);
        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(false);

        if (combatDimmer != null)
        {
            combatDimmer.gameObject.SetActive(true);
            Tween.Alpha(combatDimmer, 1f, 0.3f);
        }

        IsolateForAttack(true);

        if (attackTargetGroup != null && playerRootTransform != null && activeEnemies.Count > selectedTargetIndex && activeEnemies[selectedTargetIndex] != null)
        {
            attackTargetGroup.Targets.Clear();
            attackTargetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = playerRootTransform, Weight = 1f, Radius = 2f });
            attackTargetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = activeEnemies[selectedTargetIndex].transform, Weight = 1f, Radius = 2f });
        }

        if (vcamAttackFocus != null) vcamAttackFocus.Priority = 30;

        if (activeSkillSO != null && activeSkillSO.minigamePrefab != null)
        {
            GameObject spawnedMinigame = Instantiate(activeSkillSO.minigamePrefab);
            activeMinigameInstance = spawnedMinigame.GetComponent<ICombatMinigame>();
            
            if (playerPPSlider != null)
            {
                playerProfile.currentPP = Mathf.Clamp(playerProfile.currentPP - activeSkillSO.ppCost, 0, playerProfile.maxPP);
                Tween.UISliderValue(playerPPSlider, playerProfile.currentPP, 0.3f, Ease.OutQuad);
            }
        }
        else
        {
            Debug.LogError("No Active Skill SO or Minigame Prefab found!");
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
                        if (activeEnemies.Count > selectedTargetIndex && activeEnemies[selectedTargetIndex] != null) activeMinigameInstance.StartMinigame(activeEnemies[selectedTargetIndex].transform, activeSkillSO);
                        activeMinigameInstance.ExecuteBeat(attackMinigameBeat);
                    }
                }
                else PlaySFX(warmupSFX);
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
        IsolateForAttack(false);
        if (vcamAttackFocus != null) vcamAttackFocus.Priority = 0;

        if (damageMultiplier > 0f && playerProfile != null)
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

        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(false);
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
                if (playerHPSlider != null) Tween.UISliderValue(playerHPSlider, playerProfile.currentHP, 0.3f, Ease.OutBounce);
                
                Tween.ShakeLocalPosition(gridPlayer.transform, strength: new Vector3(25f, 25f, 0f), duration: 0.3f);
                
                if (playerProfile.currentHP <= 0 && currentState != CombatState.Defeat) TriggerGameOver();
            }
        }
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

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)) ExecuteGameOverAction(gameOverSelectedIndex);
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

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)) ExecuteVictoryAction();
    }

    private void UpdateGameOverVisuals()
    {
        if (gameOverButtons == null) return;
        for (int i = 0; i < gameOverButtons.Length; i++)
        {
            if (gameOverButtons[i] != null) gameOverButtons[i].color = (i == gameOverSelectedIndex) ? selectedActionColor : defaultActionColor;
        }
    }

    private void UpdateVictoryVisuals()
    {
        if (victoryButtons == null) return;
        for (int i = 0; i < victoryButtons.Length; i++)
        {
            if (victoryButtons[i] != null) victoryButtons[i].color = (i == victorySelectedIndex) ? selectedActionColor : defaultActionColor;
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
        if (RhythmManager.Instance != null) RhythmManager.Instance.StopTrack();
        
        CombatStatePayload.IsReturningFromCombat = true;
        CombatStatePayload.FledCombat = fled;
        CombatStatePayload.WonCombat = !fled;
        AudioListener.volume = 1f;
        
        SceneManager.LoadScene(CombatStatePayload.ReturnSceneName);
    }

    public void RestartBattle()
    {
        PendingEncounter = currentEncounter;
        if (playerProfile != null) playerProfile.currentHP = playerProfile.GetTotalMaxHP();
        if (RhythmManager.Instance != null) RhythmManager.Instance.StopTrack();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadGame() { }

    public void ReturnToMainMenu() 
    {
        SceneManager.LoadScene("MainMenu"); 
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}