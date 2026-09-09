using SaintsField;
using SaintsField.Playa;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenePhase
{
    [Separator("Condition")]
    [Required] public GameVariableSO requiredVariable;
    public ComparisonType comparison = ComparisonType.GreaterOrEqual;
    public int requiredValue = 1;

    [Separator("Target")]
    [Required] public GameObject phaseContainer;

    public bool IsConditionMet(int currentValue)
    {
        switch (comparison)
        {
            case ComparisonType.Equal: return currentValue == requiredValue;
            case ComparisonType.GreaterOrEqual: return currentValue >= requiredValue;
            case ComparisonType.LessOrEqual: return currentValue <= requiredValue;
            default: return false;
        }
    }
}

public class ScenePhaseController : MonoBehaviour
{
    [Separator("Default State")]
    public GameObject defaultPhaseContainer;

    [Separator("Story Phases (Highest Priority Last)")]
    public List<ScenePhase> storyPhases;

    void Start()
    {
        RefreshPhases();
    }

    [Button("Force Refresh Phases")]
    public void RefreshPhases()
    {
        if (defaultPhaseContainer != null) defaultPhaseContainer.SetActive(false);

        foreach (var phase in storyPhases)
        {
            if (phase.phaseContainer != null) phase.phaseContainer.SetActive(false);
        }

        GameObject activeContainer = defaultPhaseContainer;

        if (GameStateManager.Instance != null)
        {
            // Pêtla idzie od góry do do³u. Faza na samym dole listy ma najwy¿szy priorytet nadpisywania.
            foreach (var phase in storyPhases)
            {
                if (phase.requiredVariable == null) continue;

                int currentValue = GameStateManager.Instance.GetVariable(phase.requiredVariable);

                if (phase.IsConditionMet(currentValue))
                {
                    activeContainer = phase.phaseContainer;
                }
            }
        }

        if (activeContainer != null)
        {
            activeContainer.SetActive(true);
            Debug.Log($"[PhaseController] Aktywowano fazê: {activeContainer.name}");
        }
    }
}