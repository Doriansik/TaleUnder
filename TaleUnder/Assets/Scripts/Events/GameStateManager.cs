using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    private Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetFlag(string flagName, bool state)
    {
        gameFlags[flagName] = state;
    }

    public bool GetFlag(string flagName)
    {
        return gameFlags.ContainsKey(flagName) && gameFlags[flagName];
    }
}