using UnityEngine;

[CreateAssetMenu(fileName = "Var_", menuName = "Core/Game Variable")]
public class GameVariableSO : ScriptableObject
{
    [Tooltip("Aktualna wartość tej flagi (np. 0, 1, 2)")]
    public int currentValue = 0;

    [Tooltip("Notatka dla Ciebie, np. 0=Nieznane, 1=Zrobione")]
    [TextArea] public string description;
}