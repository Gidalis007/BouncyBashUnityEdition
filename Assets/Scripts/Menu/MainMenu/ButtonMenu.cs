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
        _spotLight.SetActive(true);

    }

    private void OnMouseExit()
    {
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

        _mainMenu.SetActive(!active);
        _menu.SetActive(active);
    }


}
