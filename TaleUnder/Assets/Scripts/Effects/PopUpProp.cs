using UnityEngine;
using System.Collections.Generic;
using PrimeTween;
using SaintsField;

public class PopUpGroup : MonoBehaviour
{
    [InfoBox("Assign this script ONLY to the Parent object (which has a Collider set to IsTrigger).")]
    [Separator("Elements to Animate")]

    [RichLabel("Graphic Objects (Children)")]
    [InfoBox("Drag and drop here the Sprite Renderers of all elements from the hierarchy that should appear together.")]
    public List<SpriteRenderer> elementsToAnimate = new List<SpriteRenderer>();

    [Separator("Behavior Settings")]

    [Tooltip("Check if the object should be completely invisible at the start of the game.")]
    public bool startInvisible = true;

    [Tooltip("Check if the object should fold back down when the player leaves.")]
    public bool foldBackOnExit = false;

    [RichLabel("Animation Duration")]
    [Range(0.1f, 2f)]
    public float animationDuration = 0.5f;

    private List<Quaternion> targetRotations = new List<Quaternion>();
    private bool isActivated = false;

    void Awake()
    {
        for (int i = 0; i < elementsToAnimate.Count; i++)
        {
            Transform elemTransform = elementsToAnimate[i].transform;

            targetRotations.Add(elemTransform.localRotation);

            elemTransform.localEulerAngles = new Vector3(90f, elemTransform.localEulerAngles.y, elemTransform.localEulerAngles.z);

            if (startInvisible)
            {
                Color startColor = elementsToAnimate[i].color;
                startColor.a = 0f;
                elementsToAnimate[i].color = startColor;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!foldBackOnExit && isActivated) return;

            isActivated = true;

            for (int i = 0; i < elementsToAnimate.Count; i++)
            {
                Tween.LocalRotation(elementsToAnimate[i].transform, targetRotations[i], animationDuration, Ease.OutBack);

                if (startInvisible)
                {
                    Tween.Alpha(elementsToAnimate[i], 1f, animationDuration);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (foldBackOnExit && other.CompareTag("Player"))
        {
            isActivated = false;

            for (int i = 0; i < elementsToAnimate.Count; i++)
            {
                Transform elemTransform = elementsToAnimate[i].transform;

                Vector3 flatAngle = new Vector3(90f, elemTransform.localEulerAngles.y, elemTransform.localEulerAngles.z);

                Tween.LocalRotation(elemTransform, Quaternion.Euler(flatAngle), animationDuration, Ease.InBack);

                if (startInvisible)
                {
                    Tween.Alpha(elementsToAnimate[i], 0f, animationDuration);
                }
            }
        }
    }
}