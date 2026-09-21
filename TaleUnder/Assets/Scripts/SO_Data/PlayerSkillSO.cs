using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Combat/Player Skill")]
public class PlayerSkillSO : ScriptableObject
{
    public string skillName;
    public int ppCost;
    
    [Tooltip("Prefab minigry (np. SpotHit, OsuCatch, itp.), który zostanie zespawnowany na Canvasie")]
    public GameObject minigamePrefab;
    
    [Header("Visual & Audio Feedback")]
    public Sprite successPose;
    public Sprite failPose;
    public AudioClip hitSFX;
    public AudioClip missSFX;
    public GameObject hitVFXPrefab;
}