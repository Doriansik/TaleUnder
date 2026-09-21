using UnityEngine;
using PrimeTween;

public class RhythmBouncer : MonoBehaviour
{
    public float bounceStrength = 0.2f;
    public float scaleStrength = 0.05f;
    public float duration = 0.15f;
    
    public Transform targetGraphic;

    void Start()
    {
        if (targetGraphic == null) targetGraphic = transform;

        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat += HandleBeat;
        }
    }

    void OnDestroy()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat -= HandleBeat;
        }
    }

    private void HandleBeat(int absoluteBeat, int measureBeat)
    {
        if (targetGraphic == null) return;

        Tween.PunchLocalPosition(targetGraphic, new Vector3(0f, bounceStrength, 0f), duration, 1);
        Tween.PunchScale(targetGraphic, new Vector3(scaleStrength, scaleStrength, scaleStrength), duration, 1);
    }
}