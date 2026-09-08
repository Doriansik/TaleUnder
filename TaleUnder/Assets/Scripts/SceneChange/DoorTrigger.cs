using UnityEngine;
using SaintsField;

public class DoorTrigger : MonoBehaviour
{
    [Separator("Destination")]
    [Required] public SceneReferenceSO targetScene;
    [Required] public SpawnPointKeySO targetSpawnPoint;

    [Separator("Transition Settings")]
    public FadeStyle transitionStyle = FadeStyle.CircleOut;
    public Color transitionColor = Color.black;
    [Range(0.1f, 3f)] public float transitionDuration = 0.5f;

    private bool isTransitioning = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;
            SceneDirector.Instance.TransitionToScene(targetScene, targetSpawnPoint, transitionStyle, transitionColor, transitionDuration);
        }
    }
}