using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Object = System.Object;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] private GameObject _spotLight;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _menu;
    private void OnMouseEnter()
    {
        Debug.Log("mouseenter");
        _spotLight.SetActive(true); 

    }

    private void OnMouseExit()
    {
        Debug.Log("mouseexit");
        _spotLight.SetActive(false);

    }

    private void OnMouseDown()
    {
        OnClick();
    }

    protected virtual void OnClick()
    {
       _camera.Prioritize();
       ActiveMenu(true);
    }
    
    private void ActiveMenu(bool active)
    {
        // rendre tous ça plus smooth avec des anim d'arrive et de partie
        
        _mainMenu.SetActive(!active);
        _menu.SetActive(active);
    }
    
    
}
