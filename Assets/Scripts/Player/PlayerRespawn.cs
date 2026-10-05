using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRespawn : NetworkBehaviour
{
    [SerializeField] private GameObject _canvasPrefab;
    [SerializeField] private GameObject _renderer;
    private GameObject _canvas;
    private TextMeshProUGUI _timerText;
    private Camera _playerCamera;
    private Camera _spectatorCamera;

    private Rigidbody _rb;
    private Vector3 _ogLocation;
    private Quaternion _ogRotation;
    private const string _spectateCamTag = "SpectateCam";
    private const string _spawnOneTag = "SpawnOne";
    private const string _spawnTwoTag = "SpawnTwo";
    private bool _isWaitingRespawn = false;

    [Tooltip("Durée du mode painter offert au tueur au respawn de sa 3e victime.")]
    [SerializeField] private float _painterRewardDuration = 5f;
    private CarController _pendingPainterKiller;

    private float _timer = -1f;

    private float _uiTimer = -1f;

    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();
        _ogLocation = _rb.position;
        _ogRotation = _rb.rotation;
        _playerCamera = GetComponentInChildren<Camera>();
        _spectatorCamera = FindSpectatorCamera();

        if (IsOwner)
            EnableSpectatorCamera(false);
    }


    private void Update()
    {
        if (IsServer)
            TimerRespawn();

        if (IsOwner)
        {
            TimerUI();

            if (!_isWaitingRespawn)
                EnsurePlayerCamera();
        }
    }


    public void Respawn(float time)
    {
        if (!IsServer) return;

        PlayDeathFallRpc(transform.position);

        CarController victimCar = GetComponent<CarController>();

        if (victimCar != null)
        {
            CarController killer = victimCar.GetKillCreditor();

            if (killer != null && killer.RegisterKill())
            {
                _pendingPainterKiller = killer;
                PlayKillRpc(transform.position);
            }
        }

        _timer = time;
        EnablePlayerRpc(false);

        ShowTimerRpc(time);
    }

    private void TimerRespawn()
    {
        if (_timer <= -1f) return;

        if (_timer <= 0)
        {
            _timer = -1;

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            GetRespawnPoint(out Vector3 respawnPosition, out Quaternion respawnRotation);
            _rb.position = respawnPosition;
            _rb.rotation = respawnRotation;

            EnablePlayerRpc(true);
            HideTimerRpc();

            if (_pendingPainterKiller != null)
            {
                CarController killer = _pendingPainterKiller;
                _pendingPainterKiller = null;

                if (killer != null)
                {
                    killer.EnableCarPainter(_painterRewardDuration, killer.MyTeam);
                    PlayPainterRewardRpc(killer.transform.position);
                }
            }

            return;
        }

        _timer -= Time.deltaTime;
    }

    private void TimerUI()
    {
        if (_uiTimer <= -1f) return;

        _uiTimer -= Time.deltaTime;
        float remainingTime = Mathf.Max(_uiTimer, 0f);

        if (_timerText != null)
            _timerText.text = remainingTime.ToString("0.0");
    }

    private void EnableSpectatorCamera(bool enable)
    {
        if (_playerCamera != null)
            _playerCamera.enabled = !enable;

        if (_spectatorCamera == null)
            _spectatorCamera = FindSpectatorCamera();

        if (_spectatorCamera != null)
            _spectatorCamera.enabled = enable;
    }

    private Camera FindSpectatorCamera()
    {
        try
        {
            GameObject spectatorObject = GameObject.FindGameObjectWithTag(_spectateCamTag);
            if (spectatorObject != null)
                return spectatorObject.GetComponent<Camera>();
        }
        catch (UnityException)
        {
            return null;
        }

        return null;
    }

    private void EnsurePlayerCamera()
    {
        if (_playerCamera == null)
            _playerCamera = GetComponentInChildren<Camera>();

        if (_playerCamera != null)
            _playerCamera.enabled = true;

        if (_spectatorCamera == null)
            _spectatorCamera = FindSpectatorCamera();

        if (_spectatorCamera != null)
            _spectatorCamera.enabled = false;
    }

    private void GetRespawnPoint(out Vector3 respawnPosition, out Quaternion respawnRotation)
    {
        respawnPosition = _ogLocation;
        respawnRotation = _ogRotation;

        string spawnTag = GetPlayerSpawnTag();
        if (string.IsNullOrEmpty(spawnTag))
            return;

        try
        {
            GameObject spawnObject = GameObject.FindGameObjectWithTag(spawnTag);
            if (spawnObject != null)
            {
                respawnPosition = spawnObject.transform.position;
                respawnRotation = spawnObject.transform.rotation;
            }
        }
        catch (UnityException)
        {
            return;
        }
    }

    private string GetPlayerSpawnTag()
    {
        if (OwnerClientId == 0)
            return _spawnOneTag;

        return _spawnTwoTag;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnablePlayerRpc(bool enable)
    {
        GetComponent<Collider>().enabled = enable;
        GetComponent<Rigidbody>().useGravity = enable;
        _renderer.SetActive(enable);
        GetComponent<CarController>().enabled = enable;

        if (enable && IsOwner)
            EnableSpectatorCamera(false);
    }

    [Rpc(SendTo.Owner)]
    private void ShowTimerRpc(float time)
    {
        _canvas = Instantiate(_canvasPrefab, Vector3.zero, Quaternion.identity, transform);

        foreach (var text in _canvas.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.CompareTag("Timer"))
                _timerText = text;
        }

        _uiTimer = time;
        _isWaitingRespawn = true;
        EnableSpectatorCamera(true);
    }

    [Rpc(SendTo.Owner)]
    private void HideTimerRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("respawn");
        }

        _uiTimer = -1f;
        _timerText = null;
        _isWaitingRespawn = false;

        if (_canvas != null)
            Destroy(_canvas);

        EnableSpectatorCamera(false);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDeathFallRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeathFall(position);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayKillRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAt("kill", position);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayPainterRewardRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAt("reward_painter", position);
        }
    }
}
