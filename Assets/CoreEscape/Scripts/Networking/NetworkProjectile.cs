/*
**
** File              : NetworkProjectile.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Moves a server-controlled projectile, applies
**                     damage, and despawns after impact or timeout.
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
    [SerializeField] private int damage = 25;

    private Rigidbody projectileRigidbody;
    private float despawnTime;
    private ulong shooterClientId = ulong.MaxValue;

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(ulong clientId)
    {
        shooterClientId = clientId;
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
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider hitCollider)
    {
        if (!IsServer || !IsSpawned)
        {
            return;
        }

        NetworkPlayerHealth playerHealth =
            hitCollider.GetComponentInParent<NetworkPlayerHealth>();

        if (playerHealth != null &&
            playerHealth.OwnerClientId != shooterClientId)
        {
            playerHealth.ApplyDamage(damage);
        }

        DespawnProjectile();
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