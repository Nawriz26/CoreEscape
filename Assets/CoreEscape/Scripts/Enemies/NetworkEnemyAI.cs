/*
**
** File              : NetworkEnemyAI.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Controls server-authoritative NPC pathfinding
**                     using Patrol, Chase, and Evade FSM states.
**
*/

using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemyAI : NetworkBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Evade
    }

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float evadeSpeed = 5f;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Detection")]
    [SerializeField] private float chaseDistance = 12f;
    [Range(0.1f, 0.9f)]
[SerializeField] private float evadeHealthPercentage = 0.3f;
    [SerializeField] private float safeDistance = 6f;

    [Header("References")]
    [SerializeField] private Renderer stateRenderer;

    private readonly NetworkVariable<EnemyState> currentState =
        new NetworkVariable<EnemyState>(EnemyState.Patrol);

    private NavMeshAgent agent;
    private NetworkEnemyHealth enemyHealth;
    private Vector3 startingPosition;
    private Transform targetPlayer;
    private float decisionTimer;

    private const float DecisionInterval = 0.25f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        enemyHealth = GetComponent<NetworkEnemyHealth>();

        if (stateRenderer == null)
        {
            stateRenderer = GetComponentInChildren<Renderer>();
        }
    }

    public override void OnNetworkSpawn()
    {
        currentState.OnValueChanged += OnStateChanged;
        ApplyStateColour(currentState.Value);

        if (!IsServer)
        {
            agent.enabled = false;
            return;
        }

        startingPosition = transform.position;
        ChangeState(EnemyState.Patrol);
        SetRandomPatrolDestination();
    }

    public override void OnNetworkDespawn()
    {
        currentState.OnValueChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (!IsServer ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f)
        {
            return;
        }

        decisionTimer = DecisionInterval;
        targetPlayer = FindClosestLivingPlayer();

        EvaluateState();
        PerformCurrentState();
    }

    private void EvaluateState()
{
    if (targetPlayer == null)
    {
        ChangeState(EnemyState.Patrol);
        return;
    }

    float distanceToPlayer =
        Vector3.Distance(transform.position, targetPlayer.position);

    bool hasLowHealth =
        enemyHealth != null &&
        enemyHealth.MaximumHealth > 0 &&
        (float)enemyHealth.CurrentHealth /
        enemyHealth.MaximumHealth <= evadeHealthPercentage;

    if (hasLowHealth &&
        distanceToPlayer <= chaseDistance)
    {
        ChangeState(EnemyState.Evade);
    }
    else if (distanceToPlayer <= chaseDistance)
    {
        ChangeState(EnemyState.Chase);
    }
    else
    {
        ChangeState(EnemyState.Patrol);
    }
}

    private void PerformCurrentState()
    {
        switch (currentState.Value)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Evade:
                Evade();
                break;
        }
    }

    private void Patrol()
    {
        if (!agent.hasPath ||
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            SetRandomPatrolDestination();
        }
    }

    private void Chase()
    {
        if (targetPlayer != null)
        {
            agent.SetDestination(targetPlayer.position);
        }
    }

    private void Evade()
    {
        if (targetPlayer == null)
        {
            return;
        }

        Vector3 directionAway =
            (transform.position - targetPlayer.position).normalized;

        Vector3 desiredPosition =
            transform.position + directionAway * safeDistance;

        if (NavMesh.SamplePosition(
                desiredPosition,
                out NavMeshHit hit,
                safeDistance,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomOffset =
            Random.insideUnitSphere * patrolRadius;

        randomOffset.y = 0f;

        Vector3 desiredPosition =
            startingPosition + randomOffset;

        if (NavMesh.SamplePosition(
                desiredPosition,
                out NavMeshHit hit,
                patrolRadius,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private Transform FindClosestLivingPlayer()
    {
        NetworkPlayerHealth[] players =
            FindObjectsByType<NetworkPlayerHealth>(
                FindObjectsInactive.Exclude);

        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (NetworkPlayerHealth player in players)
        {
            if (!player.IsSpawned ||
                player.CurrentHealth <= 0)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        return closestPlayer;
    }

    private void ChangeState(EnemyState newState)
    {
        currentState.Value = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                break;

            case EnemyState.Evade:
                agent.speed = evadeSpeed;
                break;
        }
    }

    private void OnStateChanged(
        EnemyState previousState,
        EnemyState newState)
    {
        ApplyStateColour(newState);
    }

    private void ApplyStateColour(EnemyState state)
    {
        if (stateRenderer == null)
        {
            return;
        }

        stateRenderer.material.color = state switch
        {
            EnemyState.Patrol => Color.cyan,
            EnemyState.Chase => Color.red,
            EnemyState.Evade => Color.yellow,
            _ => Color.white
        };
    }
}