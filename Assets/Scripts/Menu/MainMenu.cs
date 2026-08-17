using UnityEngine;

public class MainMenu : MonoBehaviour
{
    #region SerializeField

    [SerializeField] private Light _focusExitLight;
    [SerializeField] private Light _focusPlayLight;
    [SerializeField] private Light _focusCommandLight;
    [SerializeField] private Light _focusCreditsLight;
    
    #endregion SerializeField
    
    #region Attributes
    private Light[] _lights;
    #endregion Attributes
    
    #region Mono

    private void Awake()
    {
        _lights = FindObjectsOfType<Light>();
    }
    
    #endregion Mono
    
    #region Methodes

    #region Private

    private void LightSwitch(Light lightFocus, bool on)
    {
        lightFocus.intensity = on ? 1 : 0;
    }

    #endregion Private
    
    #region Public

    public void OverExitDoor()
    {
        
    }

    public void OverPlayTable()
    {
        
    }

    public void OverCommandDoor()
    {
        
    }

    public void OverCreditScreen()
    {
        
    }
        
        
    public void TurnOffLights()
    {
        foreach (Light l in _lights)
        {
            l.intensity = 0;
        }
    }

    public void TurnOnLights()
    {
        foreach (Light l in _lights)
        {
            l.intensity = 1;
        }
    }
    #endregion Public

    #endregion Methodes
}
