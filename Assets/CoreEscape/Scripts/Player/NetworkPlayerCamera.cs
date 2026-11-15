/*
**
** File              : NetworkPlayerCamera.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : November 14, 2026
** Description       : Activates the camera only for the locally
**                     owned network player and disables scene cameras.
**
*/

using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerCamera : NetworkBehaviour
{
    [Header("Player Camera")]
    [SerializeField] private GameObject playerCamera;

    private GameObject previousMainCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerCamera.SetActive(false);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        ActivateOwnerCamera();
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playerCamera != null)
        {
            playerCamera.SetActive(false);
        }

        if (previousMainCamera != null)
        {
            previousMainCamera.SetActive(true);
        }
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
        if (IsOwner)
        {
            ActivateOwnerCamera();
        }
    }

    private void ActivateOwnerCamera()
    {
        Camera[] activeCameras =
    FindObjectsByType<Camera>(
        FindObjectsInactive.Exclude
    );

        foreach (Camera activeCamera in activeCameras)
        {
            if (activeCamera.gameObject != playerCamera &&
                activeCamera.CompareTag("MainCamera"))
            {
                previousMainCamera =
                    activeCamera.gameObject;

                activeCamera.gameObject.SetActive(false);
            }
        }

        playerCamera.SetActive(true);
    }
}