/*
**
** File              : NetworkPlayerHealth.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Synchronizes player health, handles death,
**                     and respawns defeated players.
**
*/

using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float respawnDelay = 2.0f;

    private readonly NetworkVariable<int> currentHealth =
        new NetworkVariable<int>(100);

    private NetworkPlayerController playerController;
    private NetworkPlayerSpawn playerSpawn;
    private Renderer[] playerRenderers;
    private Coroutine respawnCoroutine;

    public int CurrentHealth => currentHealth.Value;
    public int MaxHealth => maxHealth;

    public event Action<int, int> HealthChanged;

    private void Awake()
    {
        playerController =
            GetComponent<NetworkPlayerController>();

        playerSpawn =
            GetComponent<NetworkPlayerSpawn>();

        playerRenderers =
            GetComponentsInChildren<Renderer>(true);
    }

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        UpdatePlayerState(currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    public void ApplyDamage(int damageAmount)
    {
        if (!IsServer ||
            currentHealth.Value <= 0 ||
            damageAmount <= 0)
        {
            return;
        }

        currentHealth.Value = Mathf.Max(
            currentHealth.Value - damageAmount,
            0
        );

        if (currentHealth.Value == 0 &&
            respawnCoroutine == null)
        {
            respawnCoroutine =
                StartCoroutine(RespawnAfterDelay());
        }
    }

    private void OnHealthChanged(
        int previousHealth,
        int newHealth)
    {
        UpdatePlayerState(newHealth);

        HealthChanged?.Invoke(
            newHealth,
            maxHealth
        );

        if (IsOwner &&
            previousHealth <= 0 &&
            newHealth > 0)
        {
            playerSpawn.MoveToAssignedSpawnPoint();
        }
    }

    private void UpdatePlayerState(int health)
    {
        bool isAlive = health > 0;

        foreach (Renderer playerRenderer in playerRenderers)
        {
            playerRenderer.enabled = isAlive;
        }

        if (IsOwner)
        {
            playerController.SetMovementEnabled(isAlive);
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (IsServer && IsSpawned)
        {
            currentHealth.Value = maxHealth;
        }

        respawnCoroutine = null;
    }
}