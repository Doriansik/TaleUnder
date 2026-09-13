using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(RectTransform))]
public class CombatGridPlayer : MonoBehaviour
{
    public Vector2Int currentGridPos = new Vector2Int(1, 1);
    public CombatGridVisuals gridVisuals;
    public float moveDuration = 0.15f;
    public float beatTolerance = 0.25f;

    private RectTransform rectTransform;
    private int lastAttemptedBeat = -1;

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SnapToPosition()
    {
        if (gridVisuals == null || gridVisuals.gridTiles.Length != 9) return;

        int index = currentGridPos.y * 3 + currentGridPos.x;
        RectTransform targetTile = gridVisuals.gridTiles[index].GetComponent<RectTransform>();

        rectTransform.position = targetTile.position;
        lastAttemptedBeat = -1;
    }

    public void UpdatePositionVisuals()
    {
        if (gridVisuals == null || gridVisuals.gridTiles.Length != 9) return;

        int index = currentGridPos.y * 3 + currentGridPos.x;
        RectTransform targetTile = gridVisuals.gridTiles[index].GetComponent<RectTransform>();

        Tween.Position(rectTransform, targetTile.position, moveDuration, Ease.OutQuad);
    }

    public void ProcessInput()
    {
        Vector2Int inputDir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) inputDir = new Vector2Int(0, 1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) inputDir = new Vector2Int(0, -1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) inputDir = new Vector2Int(-1, 0);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) inputDir = new Vector2Int(1, 0);

        if (inputDir != Vector2Int.zero)
        {
            ValidateRhythmicMove(inputDir);
        }
    }

    private void ValidateRhythmicMove(Vector2Int dir)
    {
        float songPos = RhythmManager.Instance.GetCurrentBeatPosition();
        int closestBeat = Mathf.RoundToInt(songPos);
        
        if (closestBeat <= lastAttemptedBeat) return;
        
        lastAttemptedBeat = closestBeat;
        float distance = Mathf.Abs(closestBeat - songPos);

        if (distance <= beatTolerance)
        {
            Vector2Int newPos = currentGridPos + dir;
            newPos.x = Mathf.Clamp(newPos.x, 0, 2);
            newPos.y = Mathf.Clamp(newPos.y, 0, 2);

            currentGridPos = newPos;
            UpdatePositionVisuals();
        }
        else
        {
            Tween.ShakeLocalPosition(rectTransform, strength: new Vector3(15f, 0f, 0f), duration: 0.2f);
        }
    }
}