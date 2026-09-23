using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Player/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public int levelNumber;
    public int requiredFans;
    
    public int baseMaxHP;
    public int baseMaxPP;
    public int baseAttack;
    
    public List<PlayerSkillSO> unlockedSkills = new List<PlayerSkillSO>();
}