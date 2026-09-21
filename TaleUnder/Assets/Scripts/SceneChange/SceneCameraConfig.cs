using UnityEngine;
using SaintsField;

public class SceneCameraConfig : MonoBehaviour
{
    [Separator("Editor Testing")]
    public Transform editorDebugSpawn;

    void Start()
    {
        if (SceneDirector.Instance != null && !SceneDirector.Instance.isTransitioning)
        {
            if (editorDebugSpawn != null)
            {
                SceneDirector.Instance.playerTarget.position = editorDebugSpawn.position;
            }
        }
    }

    public void ApplyConfigNow()
    {
    }
}