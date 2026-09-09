using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    private Dictionary<string, int> gameVariables = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetVariable(GameVariableSO variable, int value)
    {
        if (variable == null) return;
        gameVariables[variable.name] = value;
        Debug.Log($"[GameState] Ustawiono zmienn¹: {variable.name} = {value}");
    }

    public void AddToVariable(GameVariableSO variable, int amount)
    {
        if (variable == null) return;
        int currentValue = GetVariable(variable);
        SetVariable(variable, currentValue + amount);
    }

    public int GetVariable(GameVariableSO variable)
    {
        if (variable == null) return 0;
        return gameVariables.ContainsKey(variable.name) ? gameVariables[variable.name] : 0;
    }
}