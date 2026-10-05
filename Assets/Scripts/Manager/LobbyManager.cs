using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    #region Attributes
    private static LobbyManager _instance = null;
    [SerializeField] private GameObject _startText;
    private const int _requiredPlayerCount = 2;

    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static LobbyManager Instance { get => _instance; }



    #endregion Properties

    #region Mono

    private void Update()
    {
        UpdateStartText();

        if (!IsServer)
        {
            return;
        }

        if (HasEnoughPlayers() && Input.GetKeyDown(KeyCode.E))
        {
            StartGameRpc("GameScene");
        }
    }
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }


    #endregion Mono

    #region Methods

    private bool HasEnoughPlayers()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.PlayersController.Count >= _requiredPlayerCount;
    }

    private void UpdateStartText()
    {
        if (_startText == null)
        {
            return;
        }

        bool showText = IsServer && HasEnoughPlayers();
        if (_startText.activeSelf != showText)
        {
            _startText.SetActive(showText);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StartGameRpc(string sceneName)
    {
        if (!IsServer)
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneTransition.EnsureExists();
        NotifyTransitionRpc();
        PlayMatchMusicRpc();
        StartCoroutine(LoadSceneAfterFade(sceneName));
    }

    private IEnumerator LoadSceneAfterFade(string sceneName)
    {
        yield return new WaitForSeconds(SceneTransition.Instance.FadeDuration);

        if (NetworkManager == null || !IsServer)
        {
            yield break;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifyTransitionRpc()
    {
        SceneTransition.EnsureExists();
        SceneTransition.Instance.FadeOut();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayMatchMusicRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("match_music");
        }
    }

    #endregion Methods
}
