using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightsController : MonoBehaviour
{
    #region serializedFields
    
    [Header("Lights")]
    [SerializeField] private Light[] _pulsingLights;

    [Space(10)]
    [Header("Pulsing")]
    [SerializeField] private float _delay = 1f;
    [SerializeField] private float _minIntensity;
    [SerializeField] private float _maxIntensity = 500f;

    [Space(10)]
    [Header("Colors")]
    [SerializeField] private Color[] _colors;

    
    #endregion serializedFields
    
    #region Attributes
    
    private Light[] _lights;
    private bool _pulsingLightsActive = true;
    private float _timer = 0f;
    private static LightsController _instance = null;
    #endregion Attributes
    
    #region Properties
    
    public static LightsController Instance { get => _instance; }
    #endregion Properties
    
    #region Mono

    private void Awake()
    {
        {
            if (_instance == null)
                _instance = this;
        }

        _lights = FindObjectsOfType<Light>();
    }

    private void Start()
    {
        PulsingLights();
    }
    private void Update()
    {
        if (_timer < _delay)
        {
            _timer += Time.deltaTime; 
        }
        else
        {
            _timer = 0f;
            PulsingLights();
        }
    }

    #endregion Mono
    
    #region Methodes

    #region Private

    private void PulsingLights()
    {
        if (!_pulsingLightsActive) return;
        for (int i = 0; i < _pulsingLights.Length; i++)
        {
            _pulsingLights[i].intensity = Random.Range(_minIntensity, _maxIntensity);
            _pulsingLights[i].color = _colors[Random.Range(0, _colors.Length)];
        }
    }
    
    #endregion Private
    
    #region Public

    public void TogglePulsingLights()
    {
        _pulsingLightsActive = !_pulsingLightsActive;
        for (int i = 0; i < _pulsingLights.Length; i++)
        {
            _pulsingLights[i].intensity = 0;
        }
    }
    
    #endregion Public
    
    #endregion Methodes
    
}
