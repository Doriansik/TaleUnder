using UnityEngine;

public enum ItemEffectType { HealHP, HealPP, DamageEnemy }

[CreateAssetMenu(fileName = "NewItem", menuName = "Combat/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea(2, 3)] public string description;
    public ItemEffectType effectType;
    public int effectValue;
    public bool targetsAlly = false;
    
    [Header("Visual & Audio")]
    public AudioClip useSFX;
    public GameObject vfxPrefab;
    
    [Header("Future Systems")]
    public ScriptableObject emotionToApply; 
}