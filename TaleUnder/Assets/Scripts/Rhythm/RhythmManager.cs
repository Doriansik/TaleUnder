using UnityEngine;
using System;
using PrimeTween;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    public event Action<int, int> OnBeat;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int beatsPerMeasure = 4;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private double engineBufferTime = 0.5;

    private SongDataSO currentSongData;
    private float currentBPM = 120f;
    private float firstBeatOffset = 0f;
    private double secondsPerBeat;
    private double dspSongTime;
    private int lastRecordedBeat = -1;
    private bool isPlaying = false;
    private float baseVolume = 1f;
    
    private Tween volumeTween;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        baseVolume = musicSource.volume;
    }

    public void Update()
    {
        if (isPlaying)
        {
            double songPosition = AudioSettings.dspTime - dspSongTime - firstBeatOffset;
            
            if (songPosition < 0) return;

            float songPositionInBeats = (float)(songPosition / secondsPerBeat);
            int currentBeat = Mathf.FloorToInt(songPositionInBeats);

            if (currentBeat > lastRecordedBeat)
            {
                lastRecordedBeat = currentBeat;
                int measureBeat = (currentBeat % beatsPerMeasure) + 1;
                OnBeat?.Invoke(currentBeat, measureBeat);
            }
        }
    }

    public void PlayTrack(SongDataSO songData)
    {
        if (currentSongData == songData && isPlaying) return;

        if (isPlaying)
        {
            volumeTween.Stop();
            volumeTween = Tween.AudioVolume(musicSource, 0f, fadeDuration).OnComplete(() => StartNewTrack(songData));
        }
        else
        {
            StartNewTrack(songData);
        }
    }

    private void StartNewTrack(SongDataSO songData)
    {
        musicSource.Stop();
        
        currentSongData = songData;
        currentBPM = songData.bpm;
        firstBeatOffset = songData.firstBeatOffset;
        secondsPerBeat = 60.0 / currentBPM;
        lastRecordedBeat = -1;

        musicSource.clip = songData.track;
        dspSongTime = AudioSettings.dspTime + engineBufferTime;
        
        musicSource.volume = 0f;
        musicSource.PlayScheduled(dspSongTime);
        isPlaying = true;

        volumeTween.Stop();
        volumeTween = Tween.AudioVolume(musicSource, baseVolume, fadeDuration);
    }

    public void StopTrack()
    {
        if (!isPlaying) return;
        
        volumeTween.Stop();
        volumeTween = Tween.AudioVolume(musicSource, 0f, fadeDuration).OnComplete(() => 
        {
            isPlaying = false;
            currentSongData = null;
            musicSource.Stop();
        });
    }

    public float GetCurrentBeatPosition()
    {
        if (!isPlaying) return 0f;
        double songPosition = AudioSettings.dspTime - dspSongTime - firstBeatOffset;
        return (float)(songPosition / secondsPerBeat);
    }
}