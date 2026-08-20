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

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
    
}
