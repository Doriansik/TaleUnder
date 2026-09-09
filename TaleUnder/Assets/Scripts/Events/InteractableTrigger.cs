using UnityEngine;
using SaintsField;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour
{
    [Separator("Interaction Setup")]
    public string flagToSet;
    public bool flagValue = true;
    public bool disableSelfAfterUse = true;

    [Separator("Optional Local Events")]
    public UnityEvent onInteract;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            GameStateManager.Instance.SetFlag(flagToSet, flagValue);
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