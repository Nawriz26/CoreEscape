/*
**
** File              : NetworkScoreManager.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Stores and synchronizes the cooperative team
**                     score across the host and connected client.
**
*/

using System;
using Unity.Netcode;

public class NetworkScoreManager : NetworkBehaviour
{
    public static NetworkScoreManager Instance
    {
        get;
        private set;
    }

    private readonly NetworkVariable<int> teamScore =
        new NetworkVariable<int>(0);

    public int TeamScore => teamScore.Value;

    public event Action<int> ScoreChanged;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        teamScore.OnValueChanged += OnScoreChanged;
        ScoreChanged?.Invoke(teamScore.Value);
    }

    public override void OnNetworkDespawn()
    {
        teamScore.OnValueChanged -= OnScoreChanged;
    }

    public void AddScore(int points)
    {
        if (!IsServer || points <= 0)
        {
            return;
        }

        teamScore.Value += points;
    }

    private void OnScoreChanged(
        int previousScore,
        int newScore)
    {
        ScoreChanged?.Invoke(newScore);
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