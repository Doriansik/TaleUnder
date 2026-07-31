using UnityEngine;

public class TestRunner : MonoBehaviour
{
    #region Variables
    [SerializeField] private SongTracker songTracker;
    [SerializeField] private InputValidator inputValidator;
    [SerializeField] private KeyCode actionKey = KeyCode.Space;
    [SerializeField] private int mainBeatModulo = 4;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        songTracker.OnBeat += HandleBeat;
        songTracker.StartSong();
    }

    private void Update()
    {
        if (Input.GetKeyDown(actionKey))
        {
            HitResult result = inputValidator.ValidateHit();
            Debug.Log(result.ToString());
        }
    }

    private void OnDestroy()
    {
        if (songTracker != null)
        {
            songTracker.OnBeat -= HandleBeat;
        }
    }
    #endregion

    #region Event Handlers
    private void HandleBeat(int beatNumber)
    {
        if (beatNumber % mainBeatModulo == 0)
        {
            Debug.Log("BAM!");
        }
        else
        {
            Debug.Log("bam");
        }
    }
    #endregion
}