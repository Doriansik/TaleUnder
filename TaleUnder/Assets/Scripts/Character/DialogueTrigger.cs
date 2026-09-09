using UnityEngine;
using SaintsField;

public class DialogueTrigger : MonoBehaviour
{
    [Required] public DialogueSequenceSO dialogueToPlay;
    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueToPlay);
            }
        }
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = false; }
}