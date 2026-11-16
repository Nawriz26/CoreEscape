/*
**
** File              : NetworkRewardPickup.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Provides synchronized random health or score
**                     rewards and respawns after collection.
**
*/

using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkRewardPickup : NetworkBehaviour
{
    private enum RewardType
    {
        Health,
        Score
    }

    [Header("Reward Settings")]
    [SerializeField] private int healthAmount = 25;
    [SerializeField] private int scoreAmount = 100;
    [SerializeField] private float respawnDelay = 10f;

    [Header("References")]
    [SerializeField] private Renderer pickupRenderer;
    [SerializeField] private Collider pickupCollider;

    private readonly NetworkVariable<RewardType> rewardType =
        new NetworkVariable<RewardType>(RewardType.Health);

    private readonly NetworkVariable<bool> isAvailable =
        new NetworkVariable<bool>(true);

    private Coroutine respawnCoroutine;

    private void Awake()
    {
        if (pickupRenderer == null)
        {
            pickupRenderer = GetComponentInChildren<Renderer>();
        }

        if (pickupCollider == null)
        {
            pickupCollider = GetComponent<Collider>();
        }
    }

    public override void OnNetworkSpawn()
    {
        rewardType.OnValueChanged += OnRewardTypeChanged;
        isAvailable.OnValueChanged += OnAvailabilityChanged;

        if (IsServer)
        {
            SelectRandomReward();
            isAvailable.Value = true;
        }

        ApplyRewardColour(rewardType.Value);
        ApplyAvailability(isAvailable.Value);
    }

    public override void OnNetworkDespawn()
    {
        rewardType.OnValueChanged -= OnRewardTypeChanged;
        isAvailable.OnValueChanged -= OnAvailabilityChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !isAvailable.Value)
        {
            return;
        }

        NetworkPlayerHealth playerHealth =
            other.GetComponentInParent<NetworkPlayerHealth>();

        if (playerHealth == null ||
            playerHealth.CurrentHealth <= 0)
        {
            return;
        }

        bool rewardCollected = false;

        switch (rewardType.Value)
        {
            case RewardType.Health:
                rewardCollected =
                    playerHealth.Heal(healthAmount);
                break;

            case RewardType.Score:
                if (NetworkScoreManager.Instance != null)
                {
                    NetworkScoreManager.Instance.AddScore(
                        scoreAmount);

                    rewardCollected = true;
                }

                break;
        }

        if (rewardCollected)
        {
            BeginRespawn();
        }
    }

    private void BeginRespawn()
    {
        isAvailable.Value = false;
        ApplyAvailability(false);

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine =
            StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SelectRandomReward();
        isAvailable.Value = true;
        ApplyAvailability(true);

        respawnCoroutine = null;
    }

    private void SelectRandomReward()
    {
        rewardType.Value =
            Random.Range(0, 2) == 0
                ? RewardType.Health
                : RewardType.Score;

        ApplyRewardColour(rewardType.Value);
    }

    private void OnRewardTypeChanged(
        RewardType previousType,
        RewardType newType)
    {
        ApplyRewardColour(newType);
    }

    private void OnAvailabilityChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyAvailability(newValue);
    }

    private void ApplyRewardColour(RewardType type)
    {
        if (pickupRenderer == null)
        {
            return;
        }

        pickupRenderer.material.color =
            type == RewardType.Health
                ? Color.green
                : Color.magenta;
    }

    private void ApplyAvailability(bool available)
    {
        if (pickupRenderer != null)
        {
            pickupRenderer.enabled = available;
        }

        if (pickupCollider != null)
        {
            pickupCollider.enabled = available;
        }
    }
}