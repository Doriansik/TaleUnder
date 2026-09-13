using UnityEngine;
using UnityEngine.UI;

public class CombatGridVisuals : MonoBehaviour
{
    public Image[] gridTiles = new Image[9];
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    public void ResetAllTiles()
    {
        foreach (var tile in gridTiles)
        {
            if (tile != null) tile.color = normalColor;
        }
    }

    public void SetTileState(int x, int y, int state)
    {
        int index = y * 3 + x;
        if (index >= 0 && index < 9 && gridTiles[index] != null)
        {
            if (state == 0) gridTiles[index].color = normalColor;
            else if (state == 1) gridTiles[index].color = warningColor;
            else if (state == 2) gridTiles[index].color = dangerColor;
        }
    }
}