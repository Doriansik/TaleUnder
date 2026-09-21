using UnityEngine;
using System;

public interface ICombatMinigame
{
    Action<float> OnMinigameEnd { get; set; }
    void StartMinigame(Transform targetEnemy, PlayerSkillSO skillData);
    void ExecuteBeat(int currentMinigameBeat);
}