/*
**
** File              : GameplayHUDController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 17, 2026
** Description       : Displays local health, synchronized team score,
**                     and remaining shared team respawns.
**
*/

using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayHUDController : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text respawnsText;

    private NetworkPlayerHealth localPlayerHealth;
    private NetworkScoreManager scoreManager;
    private NetworkLevelManager levelManager;

    private IEnumerator Start()
    {
        yield return BindLocalPlayerHealth();
        yield return BindScoreManager();
        yield return BindLevelManager();
    }

    private IEnumerator BindLocalPlayerHealth()
    {
        while (localPlayerHealth == null)
        {
            NetworkPlayerHealth[] players =
                FindObjectsByType<NetworkPlayerHealth>(
                    FindObjectsInactive.Exclude);

            foreach (NetworkPlayerHealth player in players)
            {
                if (player.IsSpawned && player.IsOwner)
                {
                    localPlayerHealth = player;
                    break;
                }
            }

            if (localPlayerHealth == null)
            {
                yield return null;
            }
        }

        localPlayerHealth.HealthChanged += UpdateHealth;

        UpdateHealth(
            localPlayerHealth.CurrentHealth,
            localPlayerHealth.MaxHealth);
    }

    private IEnumerator BindScoreManager()
    {
        while (NetworkScoreManager.Instance == null ||
               !NetworkScoreManager.Instance.IsSpawned)
        {
            yield return null;
        }

        scoreManager = NetworkScoreManager.Instance;
        scoreManager.ScoreChanged += UpdateScore;

        UpdateScore(scoreManager.TeamScore);
    }

    private IEnumerator BindLevelManager()
    {
        while (NetworkLevelManager.Instance == null ||
               !NetworkLevelManager.Instance.IsSpawned)
        {
            yield return null;
        }

        levelManager = NetworkLevelManager.Instance;
        levelManager.RespawnsChanged += UpdateRespawns;

        UpdateRespawns(levelManager.RemainingTeamRespawns);
    }

    private void UpdateHealth(
        int currentHealth,
        int maximumHealth)
    {
        healthText.text =
            $"HEALTH: {currentHealth} / {maximumHealth}";
    }

    private void UpdateScore(int score)
    {
        scoreText.text = $"SCORE: {score}";
    }

    private void UpdateRespawns(int respawns)
    {
        respawnsText.text =
            $"RESPAWNS: {respawns}";
    }

    private void OnDestroy()
    {
        if (localPlayerHealth != null)
        {
            localPlayerHealth.HealthChanged -= UpdateHealth;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= UpdateScore;
        }

        if (levelManager != null)
        {
            levelManager.RespawnsChanged -= UpdateRespawns;
        }
    }
}