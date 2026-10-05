using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameStartCountdown : NetworkBehaviour
{
    [SerializeField] private GameObject _countdownCanvasPrefab;
    private GameObject _canvas;
    private TextMeshProUGUI _stepText;
    private const int _countdownStart = 3;
    private const float _startDelay = 1f;
    private const float _stepDuration = 1f;
    private const string _spawnOneTag = "SpawnOne";
    private const string _spawnTwoTag = "SpawnTwo";
    private const string _goText = "Go!";

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        TeleportPlayers();
        SetControlsRpc(true);

        yield return new WaitForSeconds(_startDelay);

        for (int step = _countdownStart; step >= 1; step--)
        {
            ShowStepRpc(step.ToString());
            yield return new WaitForSeconds(_stepDuration);
        }

        ShowStepRpc(_goText);
        SetControlsRpc(false);

        yield return new WaitForSeconds(_stepDuration);

        HideCountdownRpc();

        GameTimer gameTimer = GetComponent<GameTimer>();

        if (gameTimer != null)
        {
            gameTimer.StartMatchClock();
        }
    }

    private void TeleportPlayers()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        foreach (NetworkObjectReference playerReference in GameManager.Instance.PlayersController)
        {
            if (!playerReference.TryGet(out NetworkObject player))
            {
                continue;
            }

            string spawnTag = player.OwnerClientId == 0 ? _spawnOneTag : _spawnTwoTag;
            GameObject spawnObject = FindSpawn(spawnTag);
            if (spawnObject == null)
            {
                continue;
            }

            Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.position = spawnObject.transform.position;
                playerRigidbody.rotation = spawnObject.transform.rotation;
            }
            else
            {
                player.transform.position = spawnObject.transform.position;
                player.transform.rotation = spawnObject.transform.rotation;
            }
        }
    }

    private GameObject FindSpawn(string spawnTag)
    {
        try
        {
            return GameObject.FindGameObjectWithTag(spawnTag);
        }
        catch (UnityException)
        {
            return null;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowStepRpc(string step)
    {
        if (AudioManager.Instance != null)
        {
            if (step == _goText)
            {
                AudioManager.Instance.PlayCountdownGo();
            }
            else
            {
                AudioManager.Instance.PlaySFX("countdown_blip");
            }
        }

        if (_canvas == null)
        {
            _canvas = Instantiate(_countdownCanvasPrefab);

            foreach (TextMeshProUGUI text in _canvas.GetComponentsInChildren<TextMeshProUGUI>())
            {
                _stepText = text;
                break;
            }
        }

        if (_stepText != null)
        {
            _stepText.text = step;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetControlsRpc(bool controlsLocked)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        foreach (NetworkObjectReference playerReference in GameManager.Instance.PlayersController)
        {
            if (!playerReference.TryGet(out NetworkObject player))
            {
                continue;
            }

            CarController carController = player.GetComponent<CarController>();
            if (carController != null)
            {
                carController.enabled = !controlsLocked;
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void HideCountdownRpc()
    {
        _stepText = null;

        if (_canvas != null)
        {
            Destroy(_canvas);
        }
    }
}
