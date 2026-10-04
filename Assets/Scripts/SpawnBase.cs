using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class SpawnBase : NetworkBehaviour
{


    [SerializeField] private GameObject _playerOneSpawn;
    [SerializeField] private GameObject _playerTwoSpawn;

    [SerializeField] private float _delayBeforeSpawn = 0.1f;
    private float _timer = 0f;

    private bool _spawn = false;


    public override void OnNetworkSpawn()
    {
      _spawn = true;
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
            Debug.LogWarning("Player 1 Spawn");
            GameManager.Instance.PlayersController[0].TryGet(out NetworkObject player);
            playerSpawn = GameObject.FindGameObjectWithTag("SpawnOne");
            player.transform.position = playerSpawn.transform.position;
            player.transform.rotation = playerSpawn.transform.rotation;
        }
        else if (playerCount == 2)
        {
            GameManager.Instance.PlayersController[1].TryGet(out NetworkObject player);
            Debug.LogWarning(player);
            playerSpawn = GameObject.FindGameObjectWithTag("SpawnTwo");
            player.transform.position = playerSpawn.transform.position;
            player.transform.rotation = playerSpawn.transform.rotation;
        }
    }

    private void Update()
    {
        if (!_spawn) return;

        if (_timer <= _delayBeforeSpawn)
        {
            _timer += Time.deltaTime;

        }
        else
        {
            _timer = 0f;
            _spawn = false;
            SpawnRpc();
        }

    }
}
