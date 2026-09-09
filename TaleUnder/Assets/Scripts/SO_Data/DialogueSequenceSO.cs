using UnityEngine;
using SaintsField;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public CharacterSO character;
    public Emotion emotion = Emotion.Neutral;
    [TextArea(3, 5)] public string text;

    [Separator("Optional Scene Action")]
    public UnityEvent onLineStart;

    [Separator("Optional Variable Change")]
    public GameVariableSO flagToChange;
    public int flagValue = 1;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueSequenceSO nextSequence; 

    [Separator("Optional Variable Change")]
    public GameVariableSO flagToChange;
    public int flagValue = 1;
}

[CreateAssetMenu(fileName = "Dialogue_", menuName = "Dialogues/Sequence")]
public class DialogueSequenceSO : ScriptableObject
{
    public List<DialogueLine> lines;

    [Separator("Choices (End of Sequence)")]
    public CharacterSO playerCharacter; 
    public List<DialogueChoice> choices;
}