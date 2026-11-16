/*
**
** File              : NetworkLevelManager.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Evaluates networked level objectives, saves the
**                     final score, unlocks progression, and ends the level.
**
*/

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkLevelManager : NetworkBehaviour
{
    public static NetworkLevelManager Instance
    {
        get;
        private set;
    }

    [Header("Level Settings")]
    [SerializeField] private int difficultyIndex = 0;
    [SerializeField] private int requiredPlayers = 2;
    [SerializeField] private float objectiveCheckInterval = 0.5f;

    private readonly HashSet<ulong> playersInExtraction =
        new HashSet<ulong>();

    private float objectiveCheckTimer;
    private bool levelCompleted;
    private bool enemiesDefeatedMessageShown;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer || levelCompleted)
        {
            return;
        }

        objectiveCheckTimer -= Time.deltaTime;

        if (objectiveCheckTimer > 0f)
        {
            return;
        }

        objectiveCheckTimer = objectiveCheckInterval;
        RemoveDisconnectedPlayers();

       bool enemiesDefeated = AllEnemiesDefeated();
bool playersReady = RequiredPlayersInExtraction();

if (enemiesDefeated &&
    !enemiesDefeatedMessageShown)
{
    enemiesDefeatedMessageShown = true;
    Debug.Log(
        "Level objective: All enemies defeated.");
}

if (enemiesDefeated && playersReady)
{
    Debug.Log(
        "Level objective: Both players extracted.");

    StartCoroutine(CompleteLevel());
}
    }

    public void SetPlayerInExtraction(
        ulong clientId,
        bool isInside)
    {
        if (!IsServer || levelCompleted)
        {
            return;
        }

        if (isInside)
        {
            playersInExtraction.Add(clientId);
        }
        else
        {
            playersInExtraction.Remove(clientId);
        }

        Debug.Log(
    $"Extraction zone: {playersInExtraction.Count}" +
    $"/{requiredPlayers} players inside.");
    }

    private bool AllEnemiesDefeated()
    {
        NetworkEnemyHealth[] enemies =
            FindObjectsByType<NetworkEnemyHealth>(
                FindObjectsInactive.Exclude);

        if (enemies.Length == 0)
        {
            return false;
        }

        foreach (NetworkEnemyHealth enemy in enemies)
        {
            if (!enemy.IsDefeated)
            {
                return false;
            }
        }

        return true;
    }

    private bool RequiredPlayersInExtraction()
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.ConnectedClientsIds.Count <
            requiredPlayers)
        {
            return false;
        }

        return playersInExtraction.Count >= requiredPlayers;
    }

    private void RemoveDisconnectedPlayers()
    {
        if (NetworkManager.Singleton == null)
        {
            playersInExtraction.Clear();
            return;
        }

        playersInExtraction.RemoveWhere(
            clientId =>
                !NetworkManager.Singleton.ConnectedClients
                    .ContainsKey(clientId));
    }

    private IEnumerator CompleteLevel()
    {
        levelCompleted = true;

        int finalScore =
            NetworkScoreManager.Instance != null
                ? NetworkScoreManager.Instance.TeamScore
                : 0;

        SaveVictoryRpc(difficultyIndex, finalScore);

        yield return new WaitForSeconds(0.5f);

        NetworkManager.Singleton.SceneManager.LoadScene(
            "GameOver",
            LoadSceneMode.Single);
    }

    [Rpc(SendTo.Everyone)]
    private void SaveVictoryRpc(
        int completedDifficulty,
        int finalScore)
    {
        GameProgress.CompleteLevel(
            completedDifficulty,
            finalScore);
    }

    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnDestroy();
    }
}