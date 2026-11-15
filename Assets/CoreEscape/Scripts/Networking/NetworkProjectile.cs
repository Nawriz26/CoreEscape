/*
**
** File              : NetworkProjectile.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Moves a server-controlled projectile and
**                     despawns it after a collision or time limit.
**
*/

using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkProjectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 18.0f;
    [SerializeField] private float lifetime = 3.0f;

    private Rigidbody projectileRigidbody;
    private float despawnTime;

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        projectileRigidbody.linearVelocity =
            transform.forward * speed;

        despawnTime =
            Time.time + lifetime;
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned)
        {
            return;
        }

        if (Time.time >= despawnTime)
        {
            DespawnProjectile();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer && IsSpawned)
        {
            DespawnProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsSpawned)
        {
            DespawnProjectile();
        }
    }

    private void DespawnProjectile()
    {
        if (NetworkObject != null &&
            NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}