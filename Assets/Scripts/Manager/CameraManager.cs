using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : NetworkBehaviour
{
    #region Attributes

    [SerializeField] private CinemachineCamera _mainCamera;
    [SerializeField] private CinemachineCamera[] _cameras;
    private static CameraManager _instance = null;

    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static CameraManager Instance { get => _instance; }

    #endregion Properties

    #region Mono
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    #endregion Mono

    #region Methods

    public void SwitchCamera(CinemachineCamera camera)
    {
        camera.Priority = 1;
    }

    public void ResetCamera()
    {
        for (int i = 0; i < _cameras.Length; i++)
        {
            _cameras[i].Priority = 0;
        }
        _mainCamera.Priority = 1;
    }

    #endregion Methods
}
