using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "Combat/Enemy Data")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public GameObject enemyPrefab;
    
    public int maxHP = 15;
    public int hitsToBecomeFan = 3;
    
    public EnemyActionPatternSO[] attackPatterns;
}