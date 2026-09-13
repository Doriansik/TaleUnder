using UnityEngine;

public static class CombatStatePayload
{
    public static string ReturnSceneName = "";
    public static Vector3 ReturnPosition = Vector3.zero;
    public static GameVariableSO DefeatVariable;
    
    public static bool IsReturningFromCombat = false;
    public static bool FledCombat = false;
    public static bool WonCombat = false;
}