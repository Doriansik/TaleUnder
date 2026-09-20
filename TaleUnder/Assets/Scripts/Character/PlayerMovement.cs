using UnityEngine;
using SaintsField;

public class PlayerMovement : MonoBehaviour
{
    public static bool IsInCombat = false;
    public static bool IsInvincible = false;

    [Separator("Character Settings")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;
    
    public Renderer playerRenderer;

    private float invincibilityTimer = 0f;

    void Awake()
    {
        if (CombatStatePayload.IsReturningFromCombat)
        {
            transform.position = CombatStatePayload.ReturnPosition;
            CombatStatePayload.IsReturningFromCombat = false;

            if (CombatStatePayload.FledCombat)
            {
                StartInvincibility(3f);
                CombatStatePayload.FledCombat = false;
            }
        }
    }

    void Update()
    {
        HandleInvincibility();

        if (IsInCombat) return;
        
        // Nowa linijka blokująca ruch, gdy aktywny jest dialog z Yarn Spinnera
        if (InputManager.Instance != null && InputManager.Instance.isDialogueActive) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }

    public void StartInvincibility(float duration)
    {
        IsInvincible = true;
        invincibilityTimer = duration;
    }

    private void HandleInvincibility()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            if (playerRenderer != null)
            {
                playerRenderer.enabled = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f;
            }

            if (invincibilityTimer <= 0f)
            {
                IsInvincible = false;
                if (playerRenderer != null) playerRenderer.enabled = true;
            }
        }
    }
}