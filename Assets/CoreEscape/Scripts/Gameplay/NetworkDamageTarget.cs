/*
**
** File              : NetworkDamageTarget.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Controls a networked damageable target and awards
**                     team points when the target is destroyed.
**
*/

using Unity.Netcode;
using UnityEngine;

public class NetworkDamageTarget : NetworkBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private int maximumHealth = 50;
    [SerializeField] private int scoreValue = 100;

    private readonly NetworkVariable<int> currentHealth =
        new NetworkVariable<int>(50);

    private readonly NetworkVariable<bool> isDestroyed =
        new NetworkVariable<bool>(false);

    public int CurrentHealth => currentHealth.Value;
    public int MaximumHealth => maximumHealth;

    public override void OnNetworkSpawn()
    {
        isDestroyed.OnValueChanged += OnDestroyedStateChanged;

        if (IsServer)
        {
            currentHealth.Value = maximumHealth;
        }

        ApplyDestroyedState(isDestroyed.Value);
    }

    public override void OnNetworkDespawn()
    {
        isDestroyed.OnValueChanged -= OnDestroyedStateChanged;
    }

    public void ApplyDamage(int damage)
    {
        if (!IsServer || damage <= 0 ||
            currentHealth.Value <= 0 ||
            isDestroyed.Value)
        {
            return;
        }

        currentHealth.Value = Mathf.Max(
            0,
            currentHealth.Value - damage);

        if (currentHealth.Value == 0)
        {
            DestroyTarget();
        }
    }

    private void DestroyTarget()
    {
        if (NetworkScoreManager.Instance != null)
        {
            NetworkScoreManager.Instance.AddScore(scoreValue);
        }

        isDestroyed.Value = true;
        ApplyDestroyedState(true);
    }

    private void OnDestroyedStateChanged(
        bool previousValue,
        bool newValue)
    {
        ApplyDestroyedState(newValue);
    }

    private void ApplyDestroyedState(bool destroyed)
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
        {
            targetRenderer.enabled = !destroyed;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider targetCollider in colliders)
        {
            targetCollider.enabled = !destroyed;
        }
    }
}