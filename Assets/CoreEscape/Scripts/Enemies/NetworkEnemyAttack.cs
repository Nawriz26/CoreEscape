/*
**
** File              : NetworkEnemyAttack.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Allows a server-controlled NPC to damage the
**                     closest player when within attack range.
**
*/

using Unity.Netcode;
using UnityEngine;

public class NetworkEnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.75f;
    [SerializeField] private float attackCooldown = 1f;

    private NetworkEnemyHealth enemyHealth;
    private float cooldownTimer;

    private void Awake()
    {
        enemyHealth = GetComponent<NetworkEnemyHealth>();
    }

    private void Update()
    {
        if (!IsServer ||
            enemyHealth == null ||
            enemyHealth.IsDefeated)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
        {
            return;
        }

        NetworkPlayerHealth closestPlayer =
            FindClosestLivingPlayer();

        if (closestPlayer == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            closestPlayer.transform.position);

        if (distanceToPlayer <= attackRange)
        {
            closestPlayer.ApplyDamage(damage);
            cooldownTimer = attackCooldown;
        }
    }

    private NetworkPlayerHealth FindClosestLivingPlayer()
    {
        NetworkPlayerHealth[] players =
            FindObjectsByType<NetworkPlayerHealth>(
                FindObjectsInactive.Exclude);

        NetworkPlayerHealth closestPlayer = null;
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
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
}