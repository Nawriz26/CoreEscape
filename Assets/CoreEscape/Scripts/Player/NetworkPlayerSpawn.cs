/*
**
** File              : NetworkPlayerSpawn.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 14, 2026
** Description       : Places each locally owned network player at
**                     a different spawn point when a level loads.
**
*/

using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerSpawn : NetworkBehaviour
{
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        MoveToAssignedSpawnPoint();
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   public override void OnDestroy()
{
    SceneManager.sceneLoaded -= OnSceneLoaded;
    base.OnDestroy();
}

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        MoveToAssignedSpawnPoint();
    }

    private void MoveToAssignedSpawnPoint()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        // Do not reposition the player while in a menu scene.
        if (!sceneName.StartsWith("Level"))
        {
            return;
        }

        GameObject[] spawnPoints =
            GameObject.FindGameObjectsWithTag("PlayerSpawn");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "No PlayerSpawn objects were found in the level."
            );

            return;
        }

        Array.Sort(
            spawnPoints,
            (first, second) =>
                string.CompareOrdinal(
                    first.name,
                    second.name
                )
        );

        int spawnIndex =
            (int)(OwnerClientId % (ulong)spawnPoints.Length);

        Transform assignedSpawn =
            spawnPoints[spawnIndex].transform;

        characterController.enabled = false;

        transform.SetPositionAndRotation(
            assignedSpawn.position,
            assignedSpawn.rotation
        );

        characterController.enabled = true;

        Debug.Log(
            $"Player {OwnerClientId} spawned at " +
            $"{assignedSpawn.name}."
        );
    }
}