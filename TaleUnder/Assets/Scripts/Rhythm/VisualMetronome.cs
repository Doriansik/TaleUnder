using UnityEngine;
using PrimeTween;

public class VisualMetronome : MonoBehaviour
{
    public Transform markerContainer;
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
        
        SpawnAndTween(leftSpawn, duration);
        SpawnAndTween(rightSpawn, duration);
    }

    private void SpawnAndTween(RectTransform spawnPoint, float duration)
    {
        GameObject marker = Instantiate(markerPrefab, markerContainer);
        RectTransform rt = marker.GetComponent<RectTransform>();
        
        rt.position = spawnPoint.position;

        Tween.Position(rt, centerTarget.position, duration, Ease.Linear, useUnscaledTime: true)
             .OnComplete(() => Destroy(marker));
    }
}