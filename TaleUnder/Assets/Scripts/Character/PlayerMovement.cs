using UnityEngine;
using SaintsField;

public class PlayerMovement : MonoBehaviour

{
    [Separator("Character Settings")]

    [InfoBox("Movement speed of the character on the 3D plane (Ground).")]
    [RichLabel("Movement Speed")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    void Awake()
    {
        
    }

    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.isDialogueActive) return;
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
}