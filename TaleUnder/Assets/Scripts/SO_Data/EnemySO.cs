using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Combat/Enemy")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public int maxHP;
    public GameObject enemyPrefab;
    
    public EmotionType defaultEmotion = EmotionType.Neutral;
    public EnemyActionPatternSO defaultPattern;

    [Header("Loot & Rewards")]
    public int baseXP = 1;
    public int baseStarBits = 10;
    public int baseEncoreStars = 0;
    public ItemSO lootItem;
    [Range(0f, 1f)] public float dropChance = 0.1f;
}