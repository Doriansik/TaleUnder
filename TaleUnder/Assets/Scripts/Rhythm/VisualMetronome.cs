using UnityEngine;
using PrimeTween;

public class VisualMetronome : MonoBehaviour
{
    public RectTransform leftSpawn;
    public RectTransform rightSpawn;
    public RectTransform centerTarget;
    public GameObject markerPrefab;
    
    public int beatsToReachCenter = 2;

    private void Start()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat += SpawnMarkers;
        }
    }

    private void OnDestroy()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat -= SpawnMarkers;
        }
    }

    private void SpawnMarkers(int absoluteBeat, int measureBeat)
    {
        if (RhythmManager.Instance == null) return;
        
        float duration = RhythmManager.Instance.SecondsPerBeat * beatsToReachCenter;
        
        SpawnAndTween(leftSpawn.anchoredPosition, duration);
        SpawnAndTween(rightSpawn.anchoredPosition, duration);
    }

    private void SpawnAndTween(Vector2 startPos, float duration)
    {
        GameObject marker = Instantiate(markerPrefab, transform);
        RectTransform rt = marker.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        Tween.UIAnchoredPosition(rt, centerTarget.anchoredPosition, duration, Ease.Linear)
             .OnComplete(() => Destroy(marker));
    }
}