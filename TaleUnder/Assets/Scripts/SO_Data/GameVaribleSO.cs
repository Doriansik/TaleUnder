using UnityEngine;

[CreateAssetMenu(fileName = "Var_", menuName = "Core/Game Variable")]
public class GameVariableSO : ScriptableObject
{
    [Tooltip("Mo¿esz tu wpisaæ notatkê dla siebie, np. 0=Nieznane, 1=W trakcie, 2=Zakoñczone")]
    [TextArea] public string description;
}