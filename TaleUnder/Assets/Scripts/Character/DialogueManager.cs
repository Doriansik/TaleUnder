using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using SaintsField;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Separator("UI References")]
    [Required] public CanvasGroup dialogueCanvasGroup;
    [Required] public Image dimBackground;
    [Required] public TextMeshProUGUI nameText;
    [Required] public TextMeshProUGUI bodyText;

    [Separator("Avatars")]
    [Required] public Image npcAvatar;
    [Required] public Image playerAvatar;

    [Separator("Choices")]
    [Required] public Transform choiceContainer;
    [Required] public GameObject choiceButtonPrefab;

    [Separator("Settings")]
    public float typeSpeed = 0.02f;

    private DialogueSequenceSO currentSequence;
    private int currentLineIndex = 0;

    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private List<GameObject> activeChoiceButtons = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
        dimBackground.gameObject.SetActive(false);
        playerAvatar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // Przewijanie dialogu pod klawiszem Z
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isTyping)
            {
                // Przyspieszenie (instant display)
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                bodyText.maxVisibleCharacters = bodyText.text.Length;
                isTyping = false;
            }
            else
            {
                // Przejœcie do nastêpnej linii
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueSequenceSO sequence)
    {
        if (sequence == null || sequence.lines.Count == 0) return;

        currentSequence = sequence;
        currentLineIndex = 0;
        isDialogueActive = true;

        // Zablokowanie ruchu gracza mo¿na dodaæ tutaj (np. PlayerMovement.Instance.enabled = false;)

        dialogueCanvasGroup.interactable = true;
        dialogueCanvasGroup.blocksRaycasts = true;
        Tween.Alpha(dialogueCanvasGroup, 1f, 0.2f);

        ClearChoices();
        DisplayLine();
    }

    private void DisplayLine()
    {
        DialogueLine line = currentSequence.lines[currentLineIndex];

        // Wykonaj przypisane akcje / flagi
        line.onLineStart?.Invoke();
        if (line.flagToChange != null && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetVariable(line.flagToChange, line.flagValue);
        }

        // Setup Postaci (NPC)
        if (line.character != null)
        {
            nameText.text = line.character.characterName;
            Sprite emoSprite = line.character.GetSprite(line.emotion);
            if (emoSprite != null)
            {
                npcAvatar.sprite = emoSprite;
                npcAvatar.gameObject.SetActive(true);
                // Efekt Pop dla Avatara (Juice)
                Tween.Scale(npcAvatar.transform, Vector3.one * 1.1f, 0.1f, Ease.OutQuad)
                     .Chain(Tween.Scale(npcAvatar.transform, Vector3.one, 0.1f, Ease.InQuad));
            }
            else npcAvatar.gameObject.SetActive(false);
        }

        // Efekt pisania
        bodyText.text = line.text;
        bodyText.maxVisibleCharacters = 0;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        int totalChars = bodyText.text.Length;

        for (int i = 0; i <= totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentSequence.lines.Count)
        {
            DisplayLine();
        }
        else
        {
            if (currentSequence.choices != null && currentSequence.choices.Count > 0)
            {
                ShowChoices();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ShowChoices()
    {
        // Poka¿ Gracza i Przyciemnij t³o
        dimBackground.gameObject.SetActive(true);
        Tween.Alpha(dimBackground.GetComponent<CanvasGroup>(), 0.8f, 0.3f); // Wymaga CanvasGroup na DimBackground

        if (currentSequence.playerCharacter != null)
        {
            playerAvatar.sprite = currentSequence.playerCharacter.GetSprite(Emotion.Neutral);
            playerAvatar.gameObject.SetActive(true);
            Tween.Scale(playerAvatar.transform, Vector3.one * 1.05f, 0.15f).Chain(Tween.Scale(playerAvatar.transform, Vector3.one, 0.1f));
        }

        foreach (var choice in currentSequence.choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
            activeChoiceButtons.Add(btnObj);

            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice));

            // Animacja wjazdu przycisku
            btnObj.transform.localScale = Vector3.zero;
            Tween.Scale(btnObj.transform, Vector3.one, 0.2f, Ease.OutBack);
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        // Aplikuj flagê z wyboru
        if (choice.flagToChange != null && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetVariable(choice.flagToChange, choice.flagValue);
        }

        // Ukryj UI wyborów
        Tween.Alpha(dimBackground.GetComponent<CanvasGroup>(), 0f, 0.2f).OnComplete(() => dimBackground.gameObject.SetActive(false));
        playerAvatar.gameObject.SetActive(false);
        ClearChoices();

        if (choice.nextSequence != null) StartDialogue(choice.nextSequence);
        else EndDialogue();
    }

    private void ClearChoices()
    {
        foreach (var btn in activeChoiceButtons) Destroy(btn);
        activeChoiceButtons.Clear();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
        Tween.Alpha(dialogueCanvasGroup, 0f, 0.2f);

        // Odblokuj ruch gracza tutaj
    }
}