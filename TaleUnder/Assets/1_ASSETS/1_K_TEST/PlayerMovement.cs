using UnityEngine;
using SaintsField;

public class PlayerMovement : MonoBehaviour
{
    [Separator("Character Settings")]

    [InfoBox("Movement speed of the character on the 3D plane (Ground).")]
    [RichLabel("Movement Speed")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;

    [Separator("Animation & Visual Settings")]
    [InfoBox("Komponent Animator postaci.")]
    [Required] // Zmieniono z [RequiredField] na [Required]
    public Animator animator;

    [Tooltip("Nazwa parametru typu Bool w Animatorze Unity")]
    public string isWalkingParam = "isWalking";

    [InfoBox("Obiekt zawieraj¹cy grafiki postaci (Child). Jeœli puste, u¿yje tego obiektu.")]
    public Transform visualChild;

    private Vector3 baseScale;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        if (visualChild == null) visualChild = this.transform;

        baseScale = visualChild.localScale;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        HandleVisuals(moveX, movement);
    }

    private void HandleVisuals(float inputX, Vector3 movement)
    {
        if (animator != null)
        {
            bool isMoving = movement.sqrMagnitude > 0.001f;
            animator.SetBool(isWalkingParam, isMoving);
        }

        if (visualChild != null)
        {
            if (inputX < -0.01f)
            {
                visualChild.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
            }
            else if (inputX > 0.01f)
            {
                visualChild.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
            }
        }
    }
}