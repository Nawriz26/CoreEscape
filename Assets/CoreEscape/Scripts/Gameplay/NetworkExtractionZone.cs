/*
**
** File              : NetworkExtractionZone.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Tracks which network players are inside the
**                     level extraction area.
**
*/

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkExtractionZone : MonoBehaviour
{
    private readonly Dictionary<ulong, int> playerOverlapCounts =
        new Dictionary<ulong, int>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerRunning())
        {
            return;
        }

        NetworkPlayerHealth player =
            other.GetComponentInParent<NetworkPlayerHealth>();

        if (player == null || !player.IsSpawned)
        {
            return;
        }

        ulong clientId = player.OwnerClientId;

        playerOverlapCounts.TryGetValue(
            clientId,
            out int overlapCount);

        overlapCount++;
        playerOverlapCounts[clientId] = overlapCount;

        if (overlapCount == 1 &&
            NetworkLevelManager.Instance != null)
        {
            NetworkLevelManager.Instance
                .SetPlayerInExtraction(clientId, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServerRunning())
        {
            return;
        }

        NetworkPlayerHealth player =
            other.GetComponentInParent<NetworkPlayerHealth>();

        if (player == null)
        {
            return;
        }

        ulong clientId = player.OwnerClientId;

        if (!playerOverlapCounts.TryGetValue(
                clientId,
                out int overlapCount))
        {
            return;
        }

        overlapCount--;

        if (overlapCount <= 0)
        {
            playerOverlapCounts.Remove(clientId);

            if (NetworkLevelManager.Instance != null)
            {
                NetworkLevelManager.Instance
                    .SetPlayerInExtraction(clientId, false);
            }
        }
        else
        {
            playerOverlapCounts[clientId] = overlapCount;
        }
    }

    private static bool IsServerRunning()
    {
        return NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsServer;
    }
}