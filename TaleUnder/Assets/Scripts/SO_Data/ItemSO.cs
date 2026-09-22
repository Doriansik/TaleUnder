using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Combat/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea(2, 3)] public string description;
    public bool targetsAlly = false;

    public CombatEffectType effectType;
    public int effectValue;

    public bool appliesEmotion;
    public EmotionType emotionToApply;
    
    [Header("Visual & Audio")]
    public AudioClip useSFX;
    public GameObject vfxPrefab;
}