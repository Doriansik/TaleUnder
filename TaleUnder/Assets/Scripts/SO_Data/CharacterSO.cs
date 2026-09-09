using UnityEngine;
using System.Collections.Generic;

public enum Emotion { Neutral, Happy, Sad, Angry, Surprised, Thinking }

[System.Serializable]
public struct EmotionSprite
{
    public Emotion emotion;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "Char_", menuName = "Dialogues/Character")]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public List<EmotionSprite> sprites;

    public Sprite GetSprite(Emotion emotion)
    {
        foreach (var em in sprites)
        {
            if (em.emotion == emotion) return em.sprite;
        }
        return sprites.Count > 0 ? sprites[0].sprite : null; 
    }
}