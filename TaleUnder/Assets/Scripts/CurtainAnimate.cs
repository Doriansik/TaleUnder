using UnityEngine;

public class CurtainAnimate : MonoBehaviour
{
    [Header("Podstawowe Ustawienia")]
    [Tooltip("Maksymalny k¹t odchylenia przy najsilniejszym podmuchu (w stopniach)")]
    public float maxRotationAngle = 12.0f;

    [Tooltip("Maksymalne przesuniêcie w osi Z przy najsilniejszym podmuchu")]
    public float maxPositionOffset = 0.15f;

    [Header("Ustawienia Wiatru i Podmuchów")]
    [Tooltip("Jak czêsto pojawiaj¹ siê nowe podmuchy")]
    public float windFrequency = 0.5f;

    [Tooltip("Szybkoœæ samych drgañ/falowania materia³u podczas podmuchu")]
    public float flutterSpeed = 3.0f;

    [Tooltip("Im wy¿sza wartoœæ, tym d³u¿sze i g³êbsze s¹ momenty ciszy (np. 2-4)")]
    public float gustSharpness = 2.5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float randomOffset;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        randomOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float time = Time.time + randomOffset;

        // 1. Obliczamy si³ê podmuchu za pomoc¹ Perlin Noise (wartoœæ od 0.0 do 1.0)
        float rawWind = Mathf.PerlinNoise(time * windFrequency, 0f);

        // 2. Potêgowanie sprawia, ¿e ma³e wartoœci staj¹ siê bliskie 0 (cisza), 
        // a du¿e wartoœci zachowuj¹ swoj¹ moc (nag³y podmuch).
        float gustStrength = Mathf.Pow(rawWind, gustSharpness);

        // 3. Szybka fala sinusoidalna udaje drobne trzepotanie materia³u podczas wiatru
        float flutter = Mathf.Sin(Time.time * flutterSpeed);

        // 4. £¹czymy podmuch z drganiem (materia³ rusza siê silnie tylko przy wysokim gustStrength)
        float finalEffect = gustStrength + (flutter * 0.15f * gustStrength);

        // Aplikujemy obrót wokó³ osi X (przód / ty³)
        float currentAngle = finalEffect * maxRotationAngle;
        transform.localRotation = initialRotation * Quaternion.Euler(currentAngle, 0f, 0f);

        // Aplikujemy przesuniêcie w g³¹b (oœ Z)
        float currentOffset = finalEffect * maxPositionOffset;
        transform.localPosition = initialPosition + new Vector3(0f, 0f, currentOffset);
    }
}