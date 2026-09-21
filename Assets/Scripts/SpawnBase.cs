using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class SpawnBase : NetworkBehaviour
{

    
    [SerializeField] private GameObject _playerOneSpawn;
    [SerializeField] private GameObject _playerTwoSpawn;

    public override void OnNetworkSpawn()
    {
        SpawnRpc();
    }
    
    
   [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnRpc()
    {
        if (!IsServer)
            return;
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        
        
        GameObject playerSpawn = null;
        if (playerCount == 1)
        {
            GameManager.Instance.PlayersController[0].TryGet(out NetworkObject player);
            playerSpawn = GameObject.FindGameObjectWithTag("SpawnOne");
            player.transform.position = playerSpawn.transform.position;
            player.transform.rotation = playerSpawn.transform.rotation;
        }
        else if (playerCount == 2)
        {
            GameManager.Instance.PlayersController[1].TryGet(out NetworkObject player);
            playerSpawn = GameObject.FindGameObjectWithTag("SpawnTwo");
            player.transform.position = playerSpawn.transform.position;
            player.transform.rotation = playerSpawn.transform.rotation;
        }
    }
}
