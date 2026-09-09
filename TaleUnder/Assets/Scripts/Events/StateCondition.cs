using UnityEngine;
using SaintsField;

public class StateCondition : MonoBehaviour
{
    [Separator("Condition Settings")]
    [Required] public GameVariableSO requiredVariable;
    public ComparisonType comparison = ComparisonType.Equal;
    public int expectedValue = 1;

    public bool destroyIfConditionNotMet = false;

    void Start()
    {
        if (GameStateManager.Instance == null || requiredVariable == null) return;

        int currentValue = GameStateManager.Instance.GetVariable(requiredVariable);
        bool isMet = false;

        switch (comparison)
        {
            case ComparisonType.Equal: isMet = (currentValue == expectedValue); break;
            case ComparisonType.GreaterOrEqual: isMet = (currentValue >= expectedValue); break;
            case ComparisonType.LessOrEqual: isMet = (currentValue <= expectedValue); break;
        }

        if (!isMet)
        {
            if (destroyIfConditionNotMet) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}