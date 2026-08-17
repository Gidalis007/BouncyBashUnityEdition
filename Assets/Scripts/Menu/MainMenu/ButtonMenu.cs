using System;
using UnityEngine;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] private GameObject _spotLight;
    private void OnMouseEnter()
    {
        Debug.Log("mouseenter");
        _spotLight.SetActive(true); 
     //   LightsController.Instance.TogglePulsingLights();
    }

    private void OnMouseExit()
    {
        Debug.Log("mouseexit");
        _spotLight.SetActive(false);
      //  LightsController.Instance.TogglePulsingLights();
    }

    private void OnMouseDown()
    {
        OnClick();
    }

    protected virtual void OnClick()
    {
        Debug.Log("click");
    }
}
