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

    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static LobbyManager Instance { get => _instance; }



    #endregion Properties

    #region Mono

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartGameRpc("GameScene");
        }
    }
    private void Awake() // Makes it a singleton
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #endregion Mono

    #region Methods

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void StartGameRpc(string sceneName)
    {
        if (!IsServer)
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
    
    #endregion Methods
}