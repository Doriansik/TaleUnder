using UnityEngine;

[CreateAssetMenu(fileName = "CombatStats_Player", menuName = "Combat/Player Stats")]
public class CombatStatsSO : ScriptableObject
{
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;

    public int maxHP = 20;
    public int currentHP = 20;
    
    public int maxPP = 10;
    public int currentPP = 10;

    public int baseAttack = 5;
}