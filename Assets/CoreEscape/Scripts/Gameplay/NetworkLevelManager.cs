/*
**
** File              : NetworkLevelManager.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 17, 2026
** Description       : Evaluates networked objectives, manages shared
**                     respawns, saves results, and ends the level.
**
*/

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    [Header("Team Respawns")]
    [SerializeField] private int startingTeamRespawns = 3;

    private readonly NetworkVariable<int> remainingTeamRespawns =
        new NetworkVariable<int>(0);

    private readonly HashSet<ulong> playersInExtraction =
        new HashSet<ulong>();

    private float objectiveCheckTimer;
    private bool levelCompleted;
    private bool enemiesDefeatedMessageShown;

    public int RemainingTeamRespawns =>
        remainingTeamRespawns.Value;
    public event Action<int> RespawnsChanged;

    private void Awake()
    {
        Instance = this;
    }

   public override void OnNetworkSpawn()
{
    remainingTeamRespawns.OnValueChanged +=
        OnRemainingRespawnsChanged;

    if (IsServer)
    {
        remainingTeamRespawns.Value =
            startingTeamRespawns;
    }
}
public override void OnNetworkDespawn()
{
    remainingTeamRespawns.OnValueChanged -=
        OnRemainingRespawnsChanged;
}

private void OnRemainingRespawnsChanged(
    int previousValue,
    int newValue)
{
    RespawnsChanged?.Invoke(newValue);
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

    public bool TryConsumeTeamRespawn()
    {
        if (!IsServer || levelCompleted)
        {
            return false;
        }

        if (remainingTeamRespawns.Value <= 0)
        {
            StartDefeat();
            return false;
        }

        remainingTeamRespawns.Value--;

        Debug.Log(
            $"Team respawns remaining: " +
            $"{remainingTeamRespawns.Value}");

        return true;
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
            $"Extraction zone: " +
            $"{playersInExtraction.Count}/" +
            $"{requiredPlayers} players inside.");
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

    private void StartDefeat()
    {
        if (levelCompleted)
        {
            return;
        }

        levelCompleted = true;
        StartCoroutine(CompleteDefeat());
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

    private IEnumerator CompleteDefeat()
    {
        int finalScore =
            NetworkScoreManager.Instance != null
                ? NetworkScoreManager.Instance.TeamScore
                : 0;

        SaveDefeatRpc(finalScore);

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

    [Rpc(SendTo.Everyone)]
    private void SaveDefeatRpc(int finalScore)
    {
        GameProgress.SaveDefeat(finalScore);
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