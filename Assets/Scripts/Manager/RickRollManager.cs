using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RickRollManager : NetworkBehaviour
{
    #region Attributes
    private static RickRollManager _instance = null;

    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static RickRollManager Instance { get => _instance; }


    #endregion Properties

    #region Mono
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion Mono

    #region

    private void GetRickRolled()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=RDdQw4w9WgXcQ&start_radio=1");
    }

    public void RickRoll()
    {
        GetRickRolled();
    }
    #endregion Methods
}
