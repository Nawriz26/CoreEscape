/*
**
** File              : MainMenuController.cs
** Student           : Nawriz Ibrahim
** Student Number    : 301161181
** Course            : Game Programming 2 - COMP396 - Fall 2026
** Professor         : Diogo Fernandes de Queiroz
** Final Project     : Core Escape
** Date              : October 3, 2026
** Description       : Controls the Main Menu, scene navigation, and LAN host/client connection interface.
**
*/

using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const ushort GamePort = 7777;

    [Header("LAN Menu")]
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        // The LAN connection panel is hidden when the menu opens.
        networkPanel.SetActive(false);
        SetStatus("Ready to connect");

        if (NetworkManager.Singleton == null)
        {
            SetStatus("Network Manager was not found.");
            Debug.LogError("MainMenuController could not find NetworkManager.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void OpenNetworkPanel()
    {
        networkPanel.SetActive(true);
        SetStatus("Ready to connect");
    }

    public void CloseNetworkPanel()
    {
        networkPanel.SetActive(false);
    }

    public void OpenInstructions()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void OpenOptions()
    {
        SceneManager.LoadScene("Options");
    }

    public void StartHostGame()
    {
        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        // The host listens for LAN connections on port 7777.
        transport.SetConnectionData(
            "127.0.0.1",
            GamePort,
            "0.0.0.0"
        );

        SetStatus("Starting host...");

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            SetStatus("Host started. Waiting for Player 2...");
        }
        else
        {
            SetStatus("Unable to start host.");
        }
    }

    public void JoinGame()
    {
        string hostAddress = ipAddressInput.text.Trim();

        if (string.IsNullOrWhiteSpace(hostAddress))
        {
            SetStatus("Enter the host computer's IP address.");
            return;
        }

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(hostAddress, GamePort);

        SetStatus($"Connecting to {hostAddress}...");

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            SetStatus("Unable to start client.");
        }
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SetStatus("Host started. Waiting for Player 2...");
            }
            else
            {
                SetStatus("Player 2 connected!");
            }
        }
        else
        {
            SetStatus("Connected successfully!");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        SetStatus("Disconnected from the game.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}