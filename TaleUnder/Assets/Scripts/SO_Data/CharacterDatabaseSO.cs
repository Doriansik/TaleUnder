using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Dialogues/Character Database")]
public class CharacterDatabaseSO : ScriptableObject
{
    public CharacterDataSO[] characters;
}