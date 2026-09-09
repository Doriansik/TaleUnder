using UnityEngine;
using PrimeTween;
using SaintsField;
using System.Collections.Generic;

public enum SequenceAction { Jump, Spin180, Spin360 }

public class NPCSequence : MonoBehaviour
{
    [Separator("Pathfinding")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;

    [Separator("End Actions")]
    public List<SequenceAction> actionsAtEnd;

    public void StartSequence()
    {
        if (waypoints.Length == 0) return;

        Sequence seq = Sequence.Create();

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            float time = distance / moveSpeed;
            seq.Chain(Tween.Position(transform, waypoints[i].position, time, Ease.Linear));
        }

        seq.ChainCallback(PerformEndActions);
    }

    private void PerformEndActions()
    {
        Sequence actionSeq = Sequence.Create();

        foreach (SequenceAction action in actionsAtEnd)
        {
            switch (action)
            {
                case SequenceAction.Jump:
                    actionSeq.Chain(Tween.LocalPositionY(transform, endValue: transform.localPosition.y + 1f, duration: 0.3f, ease: Ease.OutQuad, cycles: 2, cycleMode: CycleMode.Yoyo));
                    break;
                case SequenceAction.Spin180:
                    actionSeq.Chain(Tween.LocalRotation(transform, transform.localRotation * Quaternion.Euler(0, 180, 0), 0.5f));
                    break;
                case SequenceAction.Spin360:
                    actionSeq.Chain(Tween.LocalRotation(transform, transform.localRotation * Quaternion.Euler(0, 360, 0), 1f));
                    break;
            }
        }
    }
}