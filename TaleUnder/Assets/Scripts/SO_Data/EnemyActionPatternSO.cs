using UnityEngine;

public abstract class EnemyActionPatternSO : ScriptableObject
{
    public string patternName;
    public int damageAmount = 5;
    public int totalPatternBeats = 8;
    public AudioClip warningSFX;
    public AudioClip attackSFX;

    public abstract void ExecuteBeat(int currentPatternBeat, CombatManager manager);
}