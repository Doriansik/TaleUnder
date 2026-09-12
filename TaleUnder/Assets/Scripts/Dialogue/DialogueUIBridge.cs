using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PrimeTween;
using SaintsField;
using Yarn.Unity;

public class DialogueUIBridge : MonoBehaviour
{
    public static DialogueUIBridge Instance { get; private set; }

    [Separator("Yarn Setup")]
    public YarnProject defaultYarnProject;

    [Separator("UI References")]
    [Required] public Image npcAvatar;
    [Required] public Image playerAvatar;
    [Required] public CanvasGroup dimBackground;
    [Required] public CanvasGroup linePresenterGroup;
    [Required] public CanvasGroup optionsPresenterGroup;

    [Separator("Database")]
    [Required] public CharacterDatabaseSO characterDatabase;

    private bool wereOptionsActive = false;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        npcAvatar.gameObject.SetActive(false);
        playerAvatar.gameObject.SetActive(false);
        dimBackground.alpha = 0f;
        dimBackground.gameObject.SetActive(false);
        linePresenterGroup.alpha = 0f;
        optionsPresenterGroup.alpha = 0f;
    }

    public void Start()
    {
        DialogueRunner dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            if (defaultYarnProject != null)
            {
                dialogueRunner.SetProject(defaultYarnProject);
            }

            dialogueRunner.AddCommandHandler<string, string, string>("show_avatar", ShowAvatar);
            dialogueRunner.AddCommandHandler("hide_avatars", HideAvatars);
            dialogueRunner.AddCommandHandler<string, int>("set_var", SetGameVariable);
        }
    }

    public void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.isDialogueActive)
        {
            bool areOptionsActive = optionsPresenterGroup != null && optionsPresenterGroup.alpha > 0;

            if (areOptionsActive && !wereOptionsActive)
            {
                ShowAvatar("player", "MC", "Neutral");
                Tween.Alpha(dimBackground, 1f, 0.2f);
            }
            else if (!areOptionsActive && wereOptionsActive)
            {
                Tween.Alpha(dimBackground, 0f, 0.2f);
                playerAvatar.gameObject.SetActive(false);
            }
            
            wereOptionsActive = areOptionsActive;

            if (areOptionsActive)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    Button[] allButtons = optionsPresenterGroup.GetComponentsInChildren<Button>(false);
                    if (allButtons.Length > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(allButtons[0].gameObject);
                    }
                }
            }

            if (Input.GetKeyDown(InputManager.Instance.interactKey))
            {
                if (areOptionsActive)
                {
                    GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
                    
                    if (selectedObj != null && selectedObj.transform.IsChildOf(optionsPresenterGroup.transform))
                    {
                        ExecuteEvents.Execute(selectedObj, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                        EventSystem.current.SetSelectedGameObject(null); 
                        return;
                    }
                }
                else if (linePresenterGroup != null)
                {
                    Button continueBtn = linePresenterGroup.GetComponentInChildren<Button>();
                    if (continueBtn != null)
                    {
                        continueBtn.onClick.Invoke();
                    }
                }
            }
        }
    }

    public void ShowAvatar(string side, string charID, string emotion)
    {
        Image targetAvatar = side.ToLower() == "player" ? playerAvatar : npcAvatar;
        Sprite foundSprite = null;

        if (characterDatabase != null)
        {
            foreach (var character in characterDatabase.characters)
            {
                if (character.characterID == charID)
                {
                    foreach (var emo in character.emotions)
                    {
                        if (emo.emotionName == emotion)
                        {
                            foundSprite = emo.sprite;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        if (foundSprite != null)
        {
            targetAvatar.sprite = foundSprite;
            targetAvatar.gameObject.SetActive(true);
            
            Tween.Scale(targetAvatar.transform, Vector3.one * 1.1f, 0.15f, Ease.OutQuad)
                 .Chain(Tween.Scale(targetAvatar.transform, Vector3.one, 0.1f, Ease.InQuad));
        }
    }

    public void HideAvatars()
    {
        npcAvatar.gameObject.SetActive(false);
        playerAvatar.gameObject.SetActive(false);
    }

    public void SetGameVariable(string variableName, int value)
    {
        GameVariableSO[] allVariables = Resources.LoadAll<GameVariableSO>("");
        GameVariableSO variable = null;

        foreach (var v in allVariables)
        {
            if (v.name == variableName)
            {
                variable = v;
                break;
            }
        }

        if (variable != null)
        {
            variable.currentValue = value; 
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetVariable(variable, value);
            }
        }
        else
        {
            Debug.LogError($"[Yarn] Variable missing anywhere in Resources: {variableName}");
        }
    }

    [YarnFunction("get_var")]
    public static int GetGameVariable(string variableName)
    {
        if (GameStateManager.Instance == null) return 0;

        GameVariableSO[] allVariables = Resources.LoadAll<GameVariableSO>("");
        foreach (var v in allVariables)
        {
            if (v.name == variableName)
            {
                return GameStateManager.Instance.GetVariable(v);
            }
        }
        return 0;
    }

    public void OnDialogueStart()
    {
        if (InputManager.Instance != null) InputManager.Instance.isDialogueActive = true;
        
        dimBackground.gameObject.SetActive(true);
        Tween.Alpha(dimBackground, 0f, 0.3f);
        Tween.Alpha(linePresenterGroup, 1f, 0.3f);
    }

    public void OnDialogueEnd()
    {
        if (InputManager.Instance != null) InputManager.Instance.isDialogueActive = false;

        HideAvatars();
        Tween.Alpha(dimBackground, 0f, 0.3f).OnComplete(() => dimBackground.gameObject.SetActive(false));
        Tween.Alpha(linePresenterGroup, 0f, 0.3f);
        Tween.Alpha(optionsPresenterGroup, 0f, 0.3f);
    }
}