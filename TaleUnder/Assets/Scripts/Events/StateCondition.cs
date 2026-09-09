using UnityEngine;
using SaintsField;

public class StateCondition : MonoBehaviour
{
    [Separator("Condition Settings")]
    public string requiredFlag;
    public bool expectedState = true;

    [Tooltip("If true, destroys the object. If false, just disables its renderer/collider.")]
    public bool destroyIfConditionNotMet = false;

    void Start()
    {
        if (GameStateManager.Instance == null) return;

        bool currentState = GameStateManager.Instance.GetFlag(requiredFlag);

        if (currentState != expectedState)
        {
            if (destroyIfConditionNotMet) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}