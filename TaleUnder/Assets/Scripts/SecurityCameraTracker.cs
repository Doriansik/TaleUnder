using UnityEngine;

public class SecurityCameraTracker : MonoBehaviour
{
    [Header("Cel do Œledzenia")]
    [Tooltip("Obiekt, który kamera ma obserwowaæ (np. Gracz). Jeœli puste, skrypt spróbuje znaleŸæ obiekt z tagiem 'Player'")]
    public Transform target;

    [Header("P³ynnoœæ i Szybkoœæ")]
    [Tooltip("Szybkoœæ obracania siê kamery za gracza")]
    public float rotationSpeed = 3.0f;

    [Header("Ograniczenia Ruchu (K¹ty w stopniach)")]
    [Tooltip("Czy ograniczyæ ruch kamery, aby nie obraca³a siê nienaturalnie?")]
    public bool useAngleLimits = true;

    [Tooltip("Maksymalny k¹t obrotu w lewo i prawo (od domyœlnej pozycji startowej)")]
    public float maxPanAngle = 70.0f;

    [Tooltip("Maksymalny k¹t obrotu w górê i w dó³ (od domyœlnej pozycji startowej)")]
    public float maxTiltAngle = 45.0f;

    private Quaternion initialRotation;

    void Start()
    {
        // Zapamiêtujemy pocz¹tkow¹ rotacjê z edytora/po spawnowaniu prefabu
        initialRotation = transform.rotation;

        // Jeœli cel nie zosta³ przypisany w Inspectorze, szukamy gracza po Tagu
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

        // 1. Obliczamy wektor kierunku od kamery do celu
        Vector3 directionToTarget = target.position - transform.position;

        // Jeœli gracz stoi dok³adnie w pozycji kamery, unikamy b³êdów
        if (directionToTarget.sqrMagnitude < 0.001f) return;

        // 2. Tworzymy po¿¹dan¹ rotacjê nakierowan¹ na cel
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // 3. Opcjonalne ograniczenie k¹tów (¿eby kamera nie obraca³a siê do ty³u)
        if (useAngleLimits)
        {
            // Obliczamy ró¿nicê k¹tow¹ wzglêdem domyœlnego ustawienia kamery
            Quaternion relativeRotation = Quaternion.Inverse(initialRotation) * targetRotation;
            Vector3 angles = relativeRotation.eulerAngles;

            // Konwertujemy k¹ty (0...360) na zakres (-180...180) dla ³atwiejszego ograniczenia (Clamp)
            float yaw = NormalizeAngle(angles.y);   // Obrót lewo/prawo (Pan)
            float pitch = NormalizeAngle(angles.x); // Obrót góra/dó³ (Tilt)

            // Nak³adamy limity
            yaw = Mathf.Clamp(yaw, -maxPanAngle, maxPanAngle);
            pitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);

            // Sk³adamy z powrotem ograniczon¹ rotacjê
            targetRotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        }

        // 4. P³ynny obrót kamery za pomoc¹ Slerp
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Pomocnicza metoda sprowadzaj¹ca k¹ty z range (0, 360) na (-180, 180)
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}