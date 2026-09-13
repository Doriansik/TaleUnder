using UnityEngine;
using Yarn.Unity;

public class CombatTrigger : MonoBehaviour
{
    public EncounterSO encounterSetup;
    public string combatSceneName = "CombatScene";
    public GameVariableSO defeatVariableFlag;

    private void Start()
    {
        if (CombatStatePayload.IsReturningFromCombat && 
            CombatStatePayload.WonCombat && 
            CombatStatePayload.DefeatVariable == defeatVariableFlag)
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetVariable(defeatVariableFlag, 1);
            }
            CombatStatePayload.WonCombat = false;
        }

        if (defeatVariableFlag != null && GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.GetVariable(defeatVariableFlag) >= 1)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !PlayerMovement.IsInvincible)
        {
            InitiateCombat(other.transform);
        }
    }

    public void InitiateCombat(Transform playerTransform)
    {
        CombatStatePayload.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        CombatStatePayload.ReturnPosition = playerTransform.position;
        CombatStatePayload.DefeatVariable = defeatVariableFlag;
        CombatStatePayload.IsReturningFromCombat = false;
        
        CombatManager.PendingEncounter = encounterSetup;
        UnityEngine.SceneManagement.SceneManager.LoadScene(combatSceneName);
    }
}