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
    public int currentFans = 0;
    public List<LevelDataSO> levelProgression = new List<LevelDataSO>();

    [Header("Economy")]
    public int starBits = 0;
    public int encoreStars = 0;

    [Header("Current State")]
    public int currentHP = 20;
    public int currentPP = 10;
    public EmotionType currentEmotion = EmotionType.Neutral;

    [Header("Equipment")]
    public EquipmentSO equippedMicrophone;
    public EquipmentSO equippedOutfit;
    public EquipmentSO equippedGadget;

    [Header("Abilities & Items")]
    public PlayerSkillSO basicAttack;
    public List<PlayerSkillSO> equippedSkills = new List<PlayerSkillSO>();
    public List<InventorySlot> inventory = new List<InventorySlot>();

    private LevelDataSO GetCurrentLevelData()
    {
        if (levelProgression != null && levelProgression.Count > 0)
        {
            int index = Mathf.Clamp(currentLevel - 1, 0, levelProgression.Count - 1);
            return levelProgression[index];
        }
        return null;
    }

    public int GetTotalAttack()
    {
        int total = 0;
        LevelDataSO levelData = GetCurrentLevelData();
        if (levelData != null) total += levelData.baseAttack;
        
        if (equippedMicrophone != null) total += equippedMicrophone.bonusAttack;
        if (equippedOutfit != null) total += equippedOutfit.bonusAttack;
        if (equippedGadget != null) total += equippedGadget.bonusAttack;
        return total;
    }

    public int GetTotalMaxHP()
    {
        int total = 0;
        LevelDataSO levelData = GetCurrentLevelData();
        if (levelData != null) total += levelData.baseMaxHP;
        
        if (equippedOutfit != null) total += equippedOutfit.bonusMaxHP;
        if (equippedGadget != null) total += equippedGadget.bonusMaxHP;
        return total;
    }

    public int GetTotalMaxPP()
    {
        int total = 0;
        LevelDataSO levelData = GetCurrentLevelData();
        if (levelData != null) total += levelData.baseMaxPP;
        
        if (equippedOutfit != null) total += equippedOutfit.bonusMaxPP;
        if (equippedGadget != null) total += equippedGadget.bonusMaxPP;
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

    public void AddItem(ItemSO newItem)
    {
        foreach (var slot in inventory)
        {
            if (slot.item == newItem)
            {
                slot.amount++;
                return;
            }
        }
        inventory.Add(new InventorySlot { item = newItem, amount = 1 });
    }

    public void AddFans(int amount)
    {
        currentFans += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (levelProgression == null || levelProgression.Count == 0) return;

        while (true)
        {
            int nextLevelIndex = currentLevel;
            
            if (nextLevelIndex >= levelProgression.Count) break;

            LevelDataSO nextLevelData = levelProgression[nextLevelIndex];
            
            if (currentFans >= nextLevelData.requiredFans)
            {
                currentFans -= nextLevelData.requiredFans;
                currentLevel++;
                
                currentHP = GetTotalMaxHP();
                currentPP = GetTotalMaxPP();
                
                if (nextLevelData.unlockedSkills != null)
                {
                    foreach (PlayerSkillSO skill in nextLevelData.unlockedSkills)
                    {
                        if (!equippedSkills.Contains(skill))
                        {
                            equippedSkills.Add(skill);
                        }
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    public int GetFansRequiredForNextLevel()
    {
        int nextLevelIndex = currentLevel;
        if (levelProgression != null && nextLevelIndex < levelProgression.Count)
        {
            return levelProgression[nextLevelIndex].requiredFans;
        }
        return 0;
    }
}