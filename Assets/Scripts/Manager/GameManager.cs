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

    #endregion Properties

    #region Mono
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