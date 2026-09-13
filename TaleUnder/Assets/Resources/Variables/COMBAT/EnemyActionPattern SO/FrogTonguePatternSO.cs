using UnityEngine;

[CreateAssetMenu(fileName = "Pattern_FrogTongue", menuName = "Combat/Patterns/Frog Tongue")]
public class FrogTonguePatternSO : EnemyActionPatternSO
{
    private int targetLine = 1;
    private bool isVertical = true;

    public override void ExecuteBeat(int currentPatternBeat, CombatManager manager)
    {
        int cycleBeat = ((currentPatternBeat - 1) % 4) + 1;

        if (cycleBeat == 1)
        {
            targetLine = Random.Range(0, 3);
            isVertical = Random.value > 0.5f;
            manager.PlaySFX(warningSFX);
            ShowWarning(manager, 1, false);
        }
        else if (cycleBeat == 2)
        {
            manager.PlaySFX(warningSFX);
            ShowWarning(manager, 1, false);
        }
        else if (cycleBeat == 3)
        {
            manager.PlaySFX(attackSFX);
            ShowWarning(manager, 2, true);
        }
        else if (cycleBeat == 4)
        {
            manager.gridVisuals.ResetAllTiles();
        }

        if (currentPatternBeat >= totalPatternBeats)
        {
            manager.gridVisuals.ResetAllTiles();
            manager.EndDefensePhase();
        }
    }

    private void ShowWarning(CombatManager manager, int state, bool dealDamage)
    {
        manager.gridVisuals.ResetAllTiles();
        
        for (int i = 0; i < 3; i++)
        {
            if (isVertical)
            {
                manager.gridVisuals.SetTileState(targetLine, i, state);
                if (dealDamage) manager.CheckPlayerHit(targetLine, i, damageAmount);
            }
            else
            {
                manager.gridVisuals.SetTileState(i, targetLine, state);
                if (dealDamage) manager.CheckPlayerHit(i, targetLine, damageAmount);
            }
        }
    }
}