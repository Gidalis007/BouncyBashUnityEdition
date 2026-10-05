using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenuController : NetworkBehaviour
{
    public void BackToMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI("ui_click");
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            ResetAll();
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        if (IsServer)
        {
            ReturnToMenuRpc();
        }
        else
        {
            RequestReturnToMenuServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestReturnToMenuServerRpc()
    {
        ReturnToMenuRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ReturnToMenuRpc()
    {
        ResetAll();

        NetworkManager nm = NetworkManager.Singleton;

        if (nm != null && nm.IsListening)
        {
            nm.Shutdown();
        }

        SceneManager.LoadScene("MainMenuScene");
    }

    private void ResetAll()
    {
        ScoreManager.ResetMatchWinner();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetForMenu();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EngineStop();
            AudioManager.Instance.StopAllSFX();
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic("menu_music");
        }
    }
}
