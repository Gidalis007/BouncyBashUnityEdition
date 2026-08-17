using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightManager : NetworkBehaviour
{

    #region Serialized Fields


    
    #endregion Serialized Fields
    
    #region Attributes
    private static LightManager _instance = null;
    private List<Light> _pulsingLights;
    private List<GameObject> _lightsCanOff;


    #endregion Attributes

    #region Events

    #endregion Events

    #region Properties
    public static LightManager Instance { get => _instance; }
    

    #endregion Properties

    #region Mono
    private void Awake() // Makes it a singleton
    {
        if (_instance == null)
            _instance = this;
        DontDestroyOnLoad(gameObject);
        
        GameObject[] objects = GameObject.FindGameObjectsWithTag("PulsingLight");

        for (int i = 0; i < objects.Length; i++)
        {
            Light light = objects[i].GetComponent<Light>();

            if (light != null)
                _pulsingLights.Add(light);
        }
    }

    #endregion Mono

    #region Methods

    #region Private
    

    #endregion Private
    public void InstensityLights(float percent)
    {

    }
    #endregion Methods
}