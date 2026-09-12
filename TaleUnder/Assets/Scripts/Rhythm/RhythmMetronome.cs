using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RhythmMetronome : MonoBehaviour
{
    [SerializeField] private AudioClip strongBeatSound;
    [SerializeField] private AudioClip weakBeatSound;
    [SerializeField] private bool isMuted = false;

    private AudioSource sfxSource;

    public void Awake()
    {
        sfxSource = GetComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public void Start()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat += HandleBeat;
        }
    }

    public void OnDestroy()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat -= HandleBeat;
        }
    }

    public void SetMutedState(bool state)
    {
        isMuted = state;
    }

    private void HandleBeat(int absoluteBeat, int measureBeat)
    {
        if (isMuted || sfxSource == null) return;

        AudioClip clipToPlay = (measureBeat == 1) ? strongBeatSound : weakBeatSound;

        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
    }
}