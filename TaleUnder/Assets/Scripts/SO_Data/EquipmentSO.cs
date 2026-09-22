using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Player/Equipment")]
public class EquipmentSO : ScriptableObject
{
    public string equipmentName;
    public EquipmentSlot slot;
    
    public int bonusAttack;
    public int bonusMaxHP;
    public int bonusMaxPP;
}