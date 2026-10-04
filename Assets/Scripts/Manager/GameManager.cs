using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    #region Attributes
    private static GameManager _instance = null;
    private Dictionary<ETeam, int> _teamsPoints = new Dictionary<ETeam, int>();
    private NetworkList<NetworkObjectReference> _playersController = new NetworkList<NetworkObjectReference>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static GameManager Instance { get => _instance; }

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

    public ETeam WinnerTeam { get; set; } = ETeam.NONE;

    #endregion Properties

    #region Mono

    private void Update()
    {
    }
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #endregion Mono

    #region Methods


    #endregion Methods
}
