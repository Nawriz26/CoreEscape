/*
**
** File              : NetworkPlayerAttack.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Reads attack input from the owning player
**                     and asks the server to spawn a projectile.
**
*/

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerAttack : NetworkBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack Settings")]
    [SerializeField] private float fireCooldown = 0.4f;

    private float nextFireTime;

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
        {
            return;
        }

        bool mousePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool spacePressed =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        if (!mousePressed && !spacePressed)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime =
            Time.time + fireCooldown;

        FireProjectileRpc();
    }

    [Rpc(SendTo.Server)]
    private void FireProjectileRpc()
    {
        if (projectilePrefab == null ||
            firePoint == null)
        {
            Debug.LogError(
                "Projectile Prefab or Fire Point is missing."
            );

            return;
        }

        GameObject projectileInstance =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

        NetworkProjectile networkProjectile =
    projectileInstance.GetComponent<NetworkProjectile>();

networkProjectile.Initialize(OwnerClientId);

NetworkObject projectileNetworkObject =
    projectileInstance.GetComponent<NetworkObject>();

projectileNetworkObject.Spawn();
    }
}