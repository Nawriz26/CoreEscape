/*
**
** File              : SceneNavigationController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 15, 2026
** Description       : Provides reusable menu navigation while safely
**                     cleaning up persistent network managers.
**
*/

using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationController : MonoBehaviour
{
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMenuScene("MainMenu"));
    }

    public void LoadInstructions()
    {
        StartCoroutine(LoadMenuScene("Instructions"));
    }

    public void LoadOptions()
    {
        StartCoroutine(LoadMenuScene("Options"));
    }

    private IEnumerator LoadMenuScene(string sceneName)
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null)
        {
            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            Destroy(networkManager.gameObject);

            // Wait until OnDestroy clears the singleton.
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}