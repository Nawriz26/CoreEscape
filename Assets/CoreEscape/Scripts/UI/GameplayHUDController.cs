/*
**
** File              : GameplayHUDController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 16, 2026
** Description       : Displays the local player's health and the
**                     synchronized cooperative team score.
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

    private NetworkPlayerHealth localPlayerHealth;
    private NetworkScoreManager scoreManager;

    private IEnumerator Start()
    {
        yield return BindLocalPlayerHealth();
        yield return BindScoreManager();
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

    private void UpdateHealth(int currentHealth, int maximumHealth)
    {
        healthText.text =
            $"HEALTH: {currentHealth} / {maximumHealth}";
    }

    private void UpdateScore(int score)
    {
        scoreText.text = $"SCORE: {score}";
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
    }
}