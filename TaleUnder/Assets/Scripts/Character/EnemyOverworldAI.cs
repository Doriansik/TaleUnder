using UnityEngine;
using PrimeTween;
using System.Collections.Generic;

public class EnemyOverworldAI : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 5f;
    public float chaseSpeed = 4f;
    public float chaseTime = 3f;
    public List<Transform> patrolPoints;
    public float patrolSpeed = 2f;

    private Vector3 startPosition;
    private int currentPatrolIndex = 0;
    private bool isChasing = false;
    private bool isJumping = false;
    private float chaseTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (PlayerMovement.IsInvincible) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius && !isChasing && !isJumping)
        {
            StartChase();
        }
        else if (isChasing)
        {
            ChasePlayer();
        }
        else if (!isJumping)
        {
            Patrol();
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void StartChase()
    {
        isJumping = true;
        Tween.PositionY(
            target: transform, 
            endValue: transform.position.y + 1f, 
            duration: 0.2f, 
            ease: Ease.OutQuad, 
            cycles: 4, 
            cycleMode: CycleMode.Yoyo
        ).OnComplete(() => 
        {
            isJumping = false;
            isChasing = true;
            chaseTimer = chaseTime;
        });
    }

    private void ChasePlayer()
    {
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
        {
            isChasing = false;
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, patrolSpeed * Time.deltaTime);
            return;
        }

        Transform target = patrolPoints[currentPatrolIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}