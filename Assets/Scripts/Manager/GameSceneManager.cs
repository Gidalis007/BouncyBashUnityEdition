using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameSceneManager : NetworkBehaviour
{
    
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeSceneRpc(string sceneName)
    {
        
        if (!IsServer)
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
    
}
