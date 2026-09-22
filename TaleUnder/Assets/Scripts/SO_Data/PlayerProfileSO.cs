using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int amount;
}

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "Player/Player Profile")]
public class PlayerProfileSO : ScriptableObject
{
    [Header("Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;

    [Header("Base Stats")]
    public int maxHP = 20;
    public int currentHP = 20;
    public int maxPP = 10;
    public int currentPP = 10;
    public int baseAttack = 5;

    [Header("State")]
    public EmotionType currentEmotion = EmotionType.Neutral;

    [Header("Equipment")]
    public EquipmentSO equippedMicrophone;
    public EquipmentSO equippedOutfit;
    public EquipmentSO equippedGadget;

    [Header("Abilities & Items")]
    public PlayerSkillSO basicAttack;
    public List<PlayerSkillSO> equippedSkills = new List<PlayerSkillSO>();
    public List<InventorySlot> inventory = new List<InventorySlot>();

    public int GetTotalAttack()
    {
        int total = baseAttack;
        if (equippedMicrophone != null) total += equippedMicrophone.bonusAttack;
        if (equippedOutfit != null) total += equippedOutfit.bonusAttack;
        if (equippedGadget != null) total += equippedGadget.bonusAttack;
        return total;
    }

    public int GetTotalMaxHP()
    {
        int total = maxHP;
        if (equippedOutfit != null) total += equippedOutfit.bonusMaxHP;
        if (equippedGadget != null) total += equippedGadget.bonusMaxHP;
        return total;
    }

    public void ConsumeItem(ItemSO itemToConsume)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == itemToConsume)
            {
                inventory[i].amount--;
                if (inventory[i].amount <= 0)
                {
                    inventory.RemoveAt(i);
                }
                return;
            }
        }
    }
}