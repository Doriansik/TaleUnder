using UnityEngine;
using System;

public class SongTracker : MonoBehaviour
{
    #region Events
    public event Action<int> OnBeat;
    #endregion

    #region Variables
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float beatsPerMinute;
    [SerializeField] private float firstBeatOffset;

    private float secondsInMinute = 60f;
    private float secondsPerBeat;
    private float dspSongTime;
    private float songPosition;
    private float songPositionInBeats;
    private int lastRecordedBeat = -1;
    private bool isPlaying = false;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        secondsPerBeat = secondsInMinute / beatsPerMinute;
    }

    private void Update()
    {
        if (isPlaying)
        {
            CalculateBeatPosition();
            CheckForNewBeat();
        }
    }
    #endregion

    #region Core Logic
    public void StartSong()
    {
        dspSongTime = (float)AudioSettings.dspTime;
        audioSource.Play();
        isPlaying = true;
    }

    private void CalculateBeatPosition()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime) - firstBeatOffset;
        songPositionInBeats = songPosition / secondsPerBeat;
    }

    private void CheckForNewBeat()
    {
        int currentBeat = Mathf.FloorToInt(songPositionInBeats);
        if (currentBeat > lastRecordedBeat)
        {
            lastRecordedBeat = currentBeat;
            OnBeat?.Invoke(currentBeat);
        }
    }

    public float GetSongPositionInBeats()
    {
        return songPositionInBeats;
    }
    #endregion
}