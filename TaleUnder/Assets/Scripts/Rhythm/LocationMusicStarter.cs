using UnityEngine;

public class LocationMusicStarter : MonoBehaviour
{
    public SongDataSO locationSong;

    public void Start()
    {
        if (locationSong != null && RhythmManager.Instance != null)
        {
            RhythmManager.Instance.PlayTrack(locationSong);
        }
    }
}