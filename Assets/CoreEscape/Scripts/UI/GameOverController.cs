/*
**
** File              : GameOverController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Displays the final game result and score and
**                     controls replay or return-to-menu behaviour.
**
*/

using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [Header("Game Over References")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private Button playAgainButton;

    private bool returningToMenu;
    private ulong localClientId;

    private void Start()
    {
        DisplayResult();
        ConfigureNetworkControls();
    }

    private void DisplayResult()
    {
        bool victory = GameProgress.WasVictory();
        int finalScore = GameProgress.GetFinalScore();

        resultText.text =
            victory
                ? "MISSION COMPLETE"
                : "MISSION FAILED";

        resultText.color =
            victory
                ? Color.cyan
                : new Color(1f, 0.35f, 0.35f);

        finalScoreText.text =
            $"FINAL SCORE: {finalScore}";

        if (!victory)
        {
            unlockText.text =
                "THE CORE REMAINS LOCKED";
            return;
        }

        unlockText.text =
            GameProgress.SelectedDifficulty switch
            {
                0 => "MEDIUM DIFFICULTY UNLOCKED",
                1 => "HARD DIFFICULTY UNLOCKED",
                2 => "CORE ESCAPE COMPLETE",
                _ => string.Empty
            };
    }

    private void ConfigureNetworkControls()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            !networkManager.IsListening)
        {
            playAgainButton.interactable = false;
            return;
        }

        localClientId = networkManager.LocalClientId;

        networkManager.OnClientDisconnectCallback +=
            OnClientDisconnected;

        // Only the Host controls the synchronized replay.
        playAgainButton.interactable =
            networkManager.IsServer;
    }

    public void PlayAgain()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            !networkManager.IsServer ||
            !networkManager.IsListening)
        {
            return;
        }

        networkManager.SceneManager.LoadScene(
            GameProgress.GetSelectedLevelScene(),
            LoadSceneMode.Single);
    }

   public void ReturnToMainMenu()
{
    if (returningToMenu)
    {
        return;
    }

    if (NetworkManager.Singleton == null)
    {
        returningToMenu = true;
        SceneManager.LoadScene("MainMenu");
        return;
    }

    StartCoroutine(ReturnToMainMenuRoutine());
}

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == localClientId &&
            !returningToMenu)
        {
            StartCoroutine(ReturnToMainMenuRoutine());
        }
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        returningToMenu = true;

        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;

            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            Destroy(networkManager.gameObject);
            yield return null;
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;
        }
    }
}