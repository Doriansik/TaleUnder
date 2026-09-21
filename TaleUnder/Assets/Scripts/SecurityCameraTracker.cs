using UnityEngine;

public class SecurityCameraSimple : MonoBehaviour
{
    [Header("Cel do Œledzenia")]
    public Transform target;

    [Header("Ustawienia")]
    [Tooltip("Szybkoœæ obracania siê kamery")]
    public float rotationSpeed = 3.0f;

    void Start()
    {
        // Jeœli nie przypisano celu, szukamy gracza po Tagu
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void Update()
    {
        if (target == null) return;

        // 1. Obliczamy kierunek do gracza
        Vector3 direction = target.position - transform.position;

        // Zabezpieczenie przed zerowym wektorem
        if (direction.sqrMagnitude < 0.001f) return;

        // 2. Tworzymy docelow¹ rotacjê
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 3. P³ynnie obracamy obiekt rodzica w stronê gracza
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}