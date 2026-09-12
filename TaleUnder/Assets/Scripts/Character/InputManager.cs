using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public KeyCode interactKey = KeyCode.Z;
    public KeyCode cancelKey = KeyCode.X;
    public KeyCode menuKey = KeyCode.C;

    public bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool GetInteractDown()
    {
        if (isDialogueActive) return false;
        return Input.GetKeyDown(interactKey);
    }

    public bool GetCancelDown()
    {
        if (isDialogueActive) return false;
        return Input.GetKeyDown(cancelKey);
    }

    public bool GetMenuDown()
    {
        if (isDialogueActive) return false;
        return Input.GetKeyDown(menuKey);
    }
}