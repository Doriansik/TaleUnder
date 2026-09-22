using UnityEngine;
using Yarn.Unity;

public class YarnPatch : MonoBehaviour
{
    public DialogueRunner targetRunner;
    public DialogueUIBridge targetBridge;

    private void Start()
    {
        if (targetRunner != null)
        {
            targetRunner.AddCommandHandler<string, string>("show_avatarMorfina", HandleMorfinaCommand);
        }
    }

    private void HandleMorfinaCommand(string type, string emotion)
    {
        if (targetBridge != null)
        {
            targetBridge.ShowAvatar("Morfina", type, emotion);
        }
    }
}