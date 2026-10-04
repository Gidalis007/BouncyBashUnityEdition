using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region SerializeField
    
    [SerializeField] private CinemachineCamera _mainCamera;
    [SerializeField] private GameObject _mainMenu;
    
    #endregion SerializeField
    
    #region Attributes

    #endregion Attributes
    
    #region Mono
    
    
    #endregion Mono
    
    #region Methodes

    #region Private

    private void LightSwitch(Light lightFocus, bool on)
    {
        lightFocus.intensity = on ? 1 : 0;
    }

    #endregion Private
    
    #region Public

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void ReturnBack(GameObject menu)
    {
        _mainCamera.Prioritize();
        menu.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        transform.gameObject.SetActive(false);
        NetworkManager.Singleton.SceneManager.LoadScene(
            "SampleScene",
            LoadSceneMode.Single
        );
    }

    public void Client()
    {
        NetworkManager.Singleton.StartClient();
        transform.gameObject.SetActive(false);
    }
    #endregion Public

    #endregion Methodes
}
