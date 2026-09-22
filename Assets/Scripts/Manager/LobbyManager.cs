using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    #region Attributes
    private static LobbyManager _instance = null;
    private Dictionary<ETeam, int> _teamsPoints = new Dictionary<ETeam, int>();
    private NetworkList<NetworkObjectReference> _playersController = new NetworkList<NetworkObjectReference>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static LobbyManager Instance { get => _instance; }

    public Dictionary<ETeam, int> TeamsPoints
    {
        get => _teamsPoints;
        set => _teamsPoints = value;
    }

    public NetworkList<NetworkObjectReference> PlayersController
    {
        get => _playersController;
        set => _playersController = value;
    }

    #endregion Properties

    #region Mono

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer) Debug.Log("Debug");
        
        /*for (int i = 0; i < _playersController.Count; i++)
        {
            _playersController[i].TryGet(out NetworkObject player);
            if (player.GetComponent<NetworkBehaviour>().IsServer)
            {
                Debug.Log("PlayersController Count: " + _playersController.Count);
            }
        }*/
    }
    private void Awake() // Makes it a singleton
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    #endregion Mono

    #region Methods
    
    
    #endregion Methods
}