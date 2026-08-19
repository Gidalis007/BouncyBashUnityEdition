using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TEST : NetworkBehaviour
{
    public void ChangeScene(string sceneName)
    {
        
        if (!IsServer)
        {
            Debug.LogWarningFormat(IsHost.ToString());
            Debug.LogWarningFormat(IsServer.ToString());
            return;
        }

        Debug.Log("Changing scene " + sceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
}
