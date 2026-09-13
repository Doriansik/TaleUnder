using UnityEngine;

[CreateAssetMenu(fileName = "Encounter_", menuName = "Combat/Encounter Setup")]
public class EncounterSO : ScriptableObject
{
    public string encounterName;
    public EnemySO[] enemiesInEncounter;
    public GameObject combatEnvironmentPrefab;
    public SongDataSO battleMusic;
}