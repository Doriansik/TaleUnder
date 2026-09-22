using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Combat/Player Skill")]
public class PlayerSkillSO : ScriptableObject
{
    public string skillName;
    public int ppCost;
    [TextArea(2, 3)] public string description;
    public bool targetsAlly = false;
    
    public GameObject minigamePrefab;
    
    [Header("Visual & Audio Feedback")]
    public Sprite successPose;
    public Sprite failPose;
    public AudioClip hitSFX;
    public AudioClip missSFX;
    public GameObject hitVFXPrefab;
}