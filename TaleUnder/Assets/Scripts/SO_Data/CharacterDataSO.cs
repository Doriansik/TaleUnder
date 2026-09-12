using UnityEngine;

[System.Serializable]
public struct EmotionSprite
{
    public string emotionName;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "Char_", menuName = "Dialogues/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    public string characterID;
    public EmotionSprite[] emotions;
}