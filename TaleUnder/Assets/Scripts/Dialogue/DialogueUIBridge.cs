using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using SaintsField;
using Yarn.Unity;

public class DialogueUIBridge : MonoBehaviour
{
    [Separator("UI References")]
    [Required] public Image npcAvatar;
    [Required] public Image playerAvatar;
    [Required] public CanvasGroup dimBackground;

    public void Awake()
    {
        npcAvatar.gameObject.SetActive(false);
        playerAvatar.gameObject.SetActive(false);
        dimBackground.alpha = 0f;
        dimBackground.gameObject.SetActive(false);
    }

    [YarnCommand("show_avatar")]
    public void ShowAvatar(string characterPosition, string emotionName)
    {
        Image targetAvatar = characterPosition.ToLower() == "player" ? playerAvatar : npcAvatar;
        
        targetAvatar.gameObject.SetActive(true);
        
        Tween.Scale(targetAvatar.transform, Vector3.one * 1.1f, 0.15f, Ease.OutQuad)
             .Chain(Tween.Scale(targetAvatar.transform, Vector3.one, 0.1f, Ease.InQuad));
    }

    [YarnCommand("hide_avatars")]
    public void HideAvatars()
    {
        npcAvatar.gameObject.SetActive(false);
        playerAvatar.gameObject.SetActive(false);
    }

    [YarnCommand("set_var")]
    public static void SetGameVariable(string variableName, int value)
    {
        if (GameStateManager.Instance == null) return;

        GameVariableSO variable = Resources.Load<GameVariableSO>($"Variables/{variableName}");
        if (variable != null)
        {
            GameStateManager.Instance.SetVariable(variable, value);
        }
        else
        {
            Debug.LogError($"[Yarn] Nie znaleziono zmiennej: Variables/{variableName}");
        }
    }

    public void OnDialogueStart()
    {
        dimBackground.gameObject.SetActive(true);
        Tween.Alpha(dimBackground, 0.8f, 0.3f);
    }

    public void OnDialogueEnd()
    {
        HideAvatars();
        Tween.Alpha(dimBackground, 0f, 0.3f).OnComplete(() => dimBackground.gameObject.SetActive(false));
    }
}