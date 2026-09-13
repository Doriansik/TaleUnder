using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PrimeTween;
using System.Collections.Generic;

public enum CombatState { ActionMenu, TargetSelection, AttackMinigame, DefenseGrid, Victory, Defeat }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public static EncounterSO PendingEncounter;

    public CombatState currentState = CombatState.ActionMenu;
    public EncounterSO currentEncounter;
    
    [Header("Stats & UI")]
    public CombatStatsSO playerStats;
    public Slider playerHPSlider;
    public Slider playerPPSlider;
    public Slider enemyHPSlider;
    
    [Header("Arena Setup")]
    public Transform environmentHolder;
    public Transform enemySpawnPoint;
    public Transform playerSpawnPoint;

    [Header("Action Menu References")]
    public RectTransform actionMenuContainer;
    public GameObject defenseGridContainer;
    public Image[] actionButtons;
    public Color selectedActionColor = Color.yellow;
    public Color defaultActionColor = Color.white;

    [Header("Game Over References")]
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;
    public Image[] gameOverButtons;

    [Header("Victory References")]
    public GameObject victoryPanel;
    public CanvasGroup victoryCanvasGroup;
    public Image[] victoryButtons;
    public string overworldSceneName = "OverworldScene"; // Wpisz tu nazwę swojej głównej sceny

    [Header("Attack Minigame")]
    public GameObject normalMetronomeContainer;
    public CanvasGroup combatDimmer;
    public SpotHitMinigame spotHitMinigame;
    public int warmupBeats = 2; // Ile uderzeń metronomu czekamy przed startem
    
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
    private int gameOverSelectedIndex = 0;
    private int victorySelectedIndex = 0;
    private int selectedTargetIndex = 0;
    private bool isDefending = false;
    private bool isPatternActive = false;
    private int currentPatternBeat = 0;
    
    private bool isWaitingForAttackSync = false;
    private int warmupCounter = 0;
    private int attackMinigameBeat = 0;
    
    private int maxEnemyHP;
    private int currentEnemyHP;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isRunningAway = false;
    private bool tookDamageDuringRun = false;

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
        if (RhythmManager.Instance != null) RhythmManager.Instance.OnBeat -= HandleBeat;
    }

    public void Update()
    {
        if (currentState == CombatState.ActionMenu) HandleActionMenuInput();
        else if (currentState == CombatState.TargetSelection) HandleTargetSelectionInput();
        else if (currentState == CombatState.DefenseGrid)
        {
            if (gridPlayer != null) gridPlayer.ProcessInput();
        }
        else if (currentState == CombatState.Defeat) HandleGameOverInput();
        else if (currentState == CombatState.Victory) HandleVictoryInput();
    }

    private void InitializeEncounter()
    {
        activeEnemies.Clear();

        if (playerStats != null)
        {
            if (playerHPSlider != null) { playerHPSlider.maxValue = playerStats.maxHP; playerHPSlider.value = playerStats.currentHP; }
            if (playerPPSlider != null) { playerPPSlider.maxValue = playerStats.maxPP; playerPPSlider.value = playerStats.currentPP; }
        }

        if (currentEncounter == null) return;

        if (currentEncounter.combatEnvironmentPrefab != null)
        {
            Instantiate(currentEncounter.combatEnvironmentPrefab, environmentHolder.position, environmentHolder.rotation, environmentHolder);
        }

        if (currentEncounter.enemiesInEncounter != null && currentEncounter.enemiesInEncounter.Length > 0)
        {
            EnemySO primaryEnemy = currentEncounter.enemiesInEncounter[0];
            if (primaryEnemy != null)
            {
                maxEnemyHP = primaryEnemy.maxHP;
                currentEnemyHP = maxEnemyHP;
                
                if (enemyHPSlider != null) { enemyHPSlider.maxValue = maxEnemyHP; enemyHPSlider.value = currentEnemyHP; }

                if (primaryEnemy.enemyPrefab != null)
                {
                    GameObject spawnedEnemy = Instantiate(primaryEnemy.enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
                    activeEnemies.Add(spawnedEnemy);
                }
            }
        }
    }

    public void EnterActionMenu()
    {
        currentState = CombatState.ActionMenu;
        
        if (defenseGridContainer != null) defenseGridContainer.SetActive(false);
        if (actionMenuContainer != null) actionMenuContainer.gameObject.SetActive(true);
        if (normalMetronomeContainer != null) normalMetronomeContainer.SetActive(true);
        
        if (combatDimmer != null) 
        {
            Tween.Alpha(combatDimmer, 0f, 0.3f).OnComplete(() => combatDimmer.gameObject.SetActive(false));
        }
        
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
                actionButtons[i].color = (i == selectedActionIndex) ? selectedActionColor : defaultActionColor;
        }
    }

    private void ExecuteAction(int actionIndex)
    {
        switch (actionIndex)
        {
            case 0: 
                StartTargetSelection(); 
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

    private void StartTargetSelection()
    {
        if (activeEnemies.Count == 0) return;
        currentState = CombatState.TargetSelection;
        selectedTargetIndex = 0;
        HighlightTarget();
    }

    private void HandleTargetSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedTargetIndex = (selectedTargetIndex + 1) % activeEnemies.Count;
            HighlightTarget();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedTargetIndex = (selectedTargetIndex - 1 + activeEnemies.Count) % activeEnemies.Count;
            HighlightTarget();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            ClearHighlights();
            StartAttackMinigame();
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            ClearHighlights();
            EnterActionMenu();
        }
    }

    private void HighlightTarget()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                if (i == selectedTargetIndex) Tween.Scale(activeEnemies[i].transform, Vector3.one * 1.2f, 0.2f);
                else Tween.Scale(activeEnemies[i].transform, Vector3.one, 0.2f);
            }
        }
    }

    private void ClearHighlights()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) Tween.Scale(enemy.transform, Vector3.one, 0.2f);
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
    }

    private void OnAttackMinigameEnded(float damageMultiplier)
    {
        if (damageMultiplier > 0f)
        {
            int finalDamage = Mathf.RoundToInt(playerStats.baseAttack * damageMultiplier);
            if (selectedTargetIndex >= 0 && selectedTargetIndex < activeEnemies.Count)
            {
                currentEnemyHP = Mathf.Clamp(currentEnemyHP - finalDamage, 0, maxEnemyHP);
                if (enemyHPSlider != null) Tween.UISliderValue(enemyHPSlider, currentEnemyHP, 0.3f, Ease.OutBounce);
                Tween.ShakeLocalPosition(activeEnemies[selectedTargetIndex].transform, strength: new Vector3(0.5f, 0f, 0f), duration: 0.2f);
                PlaySFX(damageSFX);
            }
        }

        if (currentEnemyHP <= 0)
        {
            TriggerVictory();
        }
        else
        {
            StartDefensePhase();
        }
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
                ExecuteVictoryAction(); 
                return;
            }
        }

        EnterActionMenu();
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
                    if (spotHitMinigame != null)
                    {
                        spotHitMinigame.OnMinigameEnd = OnAttackMinigameEnded;
                        spotHitMinigame.StartMinigame();
                        spotHitMinigame.ExecuteBeat(attackMinigameBeat);
                        PlaySFX(stepSFX);
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
                if (spotHitMinigame != null) 
                {
                    spotHitMinigame.ExecuteBeat(attackMinigameBeat);
                    if (attackMinigameBeat < 3) PlaySFX(stepSFX);
                }
            }
        }
        else if (currentState == CombatState.DefenseGrid && isDefending)
        {
            if (!isPatternActive && measureBeat == 1)
            {
                isPatternActive = true;
            }

            if (isPatternActive)
            {
                currentPatternBeat++;
                if (testPattern != null)
                {
                    testPattern.ExecuteBeat(currentPatternBeat, this);
                }
            }
        }
    }

    public void CheckPlayerHit(int gridX, int gridY, int damage)
    {
        if (gridPlayer.currentGridPos.x == gridX && gridPlayer.currentGridPos.y == gridY)
        {
            tookDamageDuringRun = true;
            PlaySFX(damageSFX);
            playerStats.currentHP = Mathf.Clamp(playerStats.currentHP - damage, 0, playerStats.maxHP);
            
            if (playerHPSlider != null) Tween.UISliderValue(playerHPSlider, playerStats.currentHP, 0.3f, Ease.OutBounce);
            Tween.ShakeLocalPosition(gridPlayer.transform, strength: new Vector3(25f, 25f, 0f), duration: 0.3f);
            
            if (playerStats.currentHP <= 0 && currentState != CombatState.Defeat) TriggerGameOver();
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
            ExecuteVictoryAction();
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
        if (playerStats != null) playerStats.currentHP = playerStats.maxHP; 
        if (RhythmManager.Instance != null) RhythmManager.Instance.StopTrack();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadGame() { }
    public void ReturnToMainMenu() { SceneManager.LoadScene("MainMenu"); }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}