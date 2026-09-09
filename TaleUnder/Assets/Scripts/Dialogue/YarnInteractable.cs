using UnityEngine;
using SaintsField;
using Yarn.Unity;

public class YarnInteractable : MonoBehaviour
{
    [Required] public string startNode = "Start";
    private bool isPlayerInRange = false;
    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.StartDialogue(startNode);
            }
        }
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = false; }
}