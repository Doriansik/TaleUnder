using UnityEngine;
using Unity.Cinemachine; // Nowa biblioteka dla wersji 3.x
using SaintsField;

public class CameraZone : MonoBehaviour
{
    [Separator("Zone Settings")]
    [Tooltip("Wirtualna kamera przypisana do tego pokoju/strefy")]
    [Required] public CinemachineCamera zoneCamera; // Nowa nazwa komponentu
    
    [Tooltip("Zaznacz, jeśli chcesz czarny ekran zamiast płynnego najazdu")]
    public bool useFadeTransition = false;
    public Color fadeColor = Color.black;
    [Range(0.1f, 2f)] public float fadeHalfDuration = 0.5f;

    private void Awake()
    {
        if (zoneCamera != null) zoneCamera.Priority = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            if (useFadeTransition)
            {
                CameraManager.Instance.PlayFadeTransition(fadeHalfDuration, fadeColor);
            }
            zoneCamera.Priority = 20; 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            if (useFadeTransition)
            {
                CameraManager.Instance.PlayFadeTransition(fadeHalfDuration, fadeColor);
            }
            zoneCamera.Priority = 0;
        }
    }
}