using UnityEngine;

[CreateAssetMenu(fileName = "Song_", menuName = "Audio/Song Data")]
public class SongDataSO : ScriptableObject
{
    public string songID;
    public AudioClip track;
    public float bpm = 120f;
    public float firstBeatOffset = 0f;
}