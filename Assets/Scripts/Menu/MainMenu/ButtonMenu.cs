using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] private GameObject _spotLight;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private GameObject _menu;
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
       _camera.Prioritize();
       ToggleMenu();

      //  yield return new WaitForSeconds(1f);

        // Rajouté un système d'animation d'entrée pour le menu
      //  _menu?.SetActive(true);
    }
    
    private void ToggleMenu()
    {
        Debug.Log(_menu.activeSelf);
        _menu.SetActive(!_menu.activeSelf);
    }
    
    
}
