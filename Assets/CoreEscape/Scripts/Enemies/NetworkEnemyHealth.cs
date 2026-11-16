/*
**
** File              : NetworkEnemyHealth.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Synchronizes NPC health, defeat state, visual
**                     removal, and score rewards across the network.
**
*/

using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkEnemyHealth : NetworkBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] private int maximumHealth = 100;
    [SerializeField] private int scoreValue = 250;

    private readonly NetworkVariable<int> currentHealth =
        new NetworkVariable<int>(100);

    private readonly NetworkVariable<bool> isDefeated =
        new NetworkVariable<bool>(false);

    public int CurrentHealth => currentHealth.Value;
    public int MaximumHealth => maximumHealth;
    public bool IsDefeated => isDefeated.Value;

    public override void OnNetworkSpawn()
    {
        isDefeated.OnValueChanged += OnDefeatedStateChanged;

        if (IsServer)
        {
            currentHealth.Value = maximumHealth;
            isDefeated.Value = false;
        }

        ApplyDefeatedState(isDefeated.Value);
    }

    public override void OnNetworkDespawn()
    {
        isDefeated.OnValueChanged -= OnDefeatedStateChanged;
    }

    public void ApplyDamage(int damage)
    {
        if (!IsServer ||
            damage <= 0 ||
            isDefeated.Value)
        {
            return;
        }

        currentHealth.Value = Mathf.Max(
            0,
            currentHealth.Value - damage);

        if (currentHealth.Value == 0)
        {
            DefeatEnemy();
        }
    }

    private void DefeatEnemy()
    {
        if (NetworkScoreManager.Instance != null)
        {
            NetworkScoreManager.Instance.AddScore(scoreValue);
        }

        isDefeated.Value = true;
        ApplyDefeatedState(true);
    }

    private void OnDefeatedStateChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyDefeatedState(newValue);
    }

    private void ApplyDefeatedState(bool defeated)
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer enemyRenderer in renderers)
        {
            enemyRenderer.enabled = !defeated;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider enemyCollider in colliders)
        {
            enemyCollider.enabled = !defeated;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (defeated && agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            agent.enabled = false;
        }
    }
}