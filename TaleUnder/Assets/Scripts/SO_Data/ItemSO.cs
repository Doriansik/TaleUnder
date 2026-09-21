using UnityEngine;

public enum ItemEffectType { HealHP, HealPP, DamageEnemy }

[CreateAssetMenu(fileName = "NewItem", menuName = "Combat/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public ItemEffectType effectType;
    public int effectValue;
    
    [Header("Visual & Audio")]
    public AudioClip useSFX;
    public GameObject vfxPrefab;
    
    [Header("Future Systems")]
    [Tooltip("Miejsce na przyszły obiekt EmotionSO")]
    public ScriptableObject emotionToApply; 
}