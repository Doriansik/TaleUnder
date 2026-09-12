using UnityEngine;

public enum HitResult
{
    Perfect,
    Good,
    Miss
}

public class InputValidator : MonoBehaviour
{
    #region Variables
    [SerializeField] private SongTracker songTracker;
    [SerializeField] private float perfectWindow;
    [SerializeField] private float goodWindow;
    [SerializeField] private int targetBeatModulo;
    #endregion

    #region Core Logic
    public HitResult ValidateHit()
    {
        float currentBeatPosition = songTracker.GetSongPositionInBeats();
        float distanceToNextBeat = CalculateDistanceToClosestTargetBeat(currentBeatPosition);

        if (distanceToNextBeat <= perfectWindow)
        {
            return HitResult.Perfect;
        }

        if (distanceToNextBeat <= goodWindow)
        {
            return HitResult.Good;
        }

        return HitResult.Miss;
    }

    private float CalculateDistanceToClosestTargetBeat(float currentPosition)
    {
        float closestTarget = Mathf.Round(currentPosition / targetBeatModulo) * targetBeatModulo;
        return Mathf.Abs(closestTarget - currentPosition);
    }
    #endregion
}