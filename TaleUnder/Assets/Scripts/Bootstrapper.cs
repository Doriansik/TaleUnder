using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        if (GameObject.Find("[GAME_CORE]") != null)
        {
            return;
        }

        GameObject corePrefab = Resources.Load<GameObject>("[GAME_CORE]");

        if (corePrefab == null)
        {
            Debug.LogError("Bootstrapper cannot find [GAME_CORE] in the Resources folder!");
            return;
        }

        GameObject coreInstance = Object.Instantiate(corePrefab);
        coreInstance.name = "[GAME_CORE]";
        Object.DontDestroyOnLoad(coreInstance);
    }
}