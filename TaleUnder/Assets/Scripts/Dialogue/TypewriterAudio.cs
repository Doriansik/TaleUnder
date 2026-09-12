using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(AudioSource))]
public class TypewriterAudio : MonoBehaviour
{
    public CharacterDatabaseSO characterDatabase;
    public TextMeshProUGUI nameField;
    public TextMeshProUGUI lineText;
    public AudioClip defaultTypeSound;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource audioSource;
    private string lastSpeakerName = "";
    private string currentLineText = "";
    private AudioClip currentTypeSound;
    private int lastVisibleChars = 0;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        currentTypeSound = defaultTypeSound;
    }

    public void Update()
    {
        if (lineText == null) return;

        if (lineText.text != currentLineText)
        {
            currentLineText = lineText.text;
            lastVisibleChars = 0;
        }

        int currentVisibleChars = lineText.maxVisibleCharacters;

        if (currentVisibleChars > lastVisibleChars && currentVisibleChars <= lineText.textInfo.characterCount)
        {
            PlayTypeSound();
            lastVisibleChars = currentVisibleChars;
        }
    }

    private void PlayTypeSound()
    {
        string cleanName = "";
        if (nameField != null && !string.IsNullOrEmpty(nameField.text))
        {
            cleanName = nameField.text.Replace("\u200B", "").Trim();
        }

        if (cleanName != lastSpeakerName)
        {
            lastSpeakerName = cleanName;
            UpdateSound();
        }

        if (currentTypeSound == null) return;
        
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(currentTypeSound);
    }

    private void UpdateSound()
    {
        currentTypeSound = defaultTypeSound;

        if (characterDatabase == null || string.IsNullOrEmpty(lastSpeakerName)) return;

        foreach (var character in characterDatabase.characters)
        {
            if (string.Equals(character.characterID, lastSpeakerName, StringComparison.OrdinalIgnoreCase))
            {
                if (character.typingSound != null)
                {
                    currentTypeSound = character.typingSound;
                }
                break;
            }
        }
    }
}