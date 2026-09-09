using UnityEngine;
using SaintsField;
using UnityEngine.Events;

public enum VariableAction { SetValue, AddValue }

public class InteractableTrigger : MonoBehaviour
{
    [Separator("Interaction Setup")]
    [Required] public GameVariableSO targetVariable;
    public VariableAction action = VariableAction.SetValue;
    public int valueToApply = 1;
    public bool disableSelfAfterUse = true;

    [Separator("Optional Local Events")]
    public UnityEvent onInteract;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            if (targetVariable != null && GameStateManager.Instance != null)
            {
                if (action == VariableAction.SetValue)
                    GameStateManager.Instance.SetVariable(targetVariable, valueToApply);
                else if (action == VariableAction.AddValue)
                    GameStateManager.Instance.AddToVariable(targetVariable, valueToApply);
            }

            onInteract?.Invoke();

            if (disableSelfAfterUse) gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}