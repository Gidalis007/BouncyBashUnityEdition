using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Attributes
    private static GameManager _instance = null;
    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static GameManager Instance { get => _instance; }
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