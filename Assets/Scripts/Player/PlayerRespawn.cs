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
    
    // Timer "physique" : n'existe et n'agit que côté serveur (repositionnement du Rigidbody, comme CarController)
    private float _timer = -1f;
    
    // Timer "visuel" : copie locale utilisée uniquement par le owner pour l'affichage du texte
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
        // Seul le serveur a l'autorité sur le Rigidbody désormais -> seul lui fait le vrai repositionnement
        if (IsServer)
            TimerRespawn();

        // Le owner gère en parallèle sa propre copie du timer, uniquement pour l'affichage
        if (IsOwner)
        {
            TimerUI();

            // Rattrapage lobby : la SpectateCam de scène (depth supérieure) peut apparaître
            // après le spawn réseau et couvrir la caméra joueur. Tant qu'on n'attend pas
            // de respawn, on force la caméra joueur chez l'owner.
            if (!_isWaitingRespawn)
                EnsurePlayerCamera();
        }
    }


    // A appeler côté serveur (ex: détection de chute faite côté serveur, seul fiable niveau physique maintenant)
    public void Respawn(float time)
    {
        if (!IsServer) return;

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
            
            return;
        }

        _timer -= Time.deltaTime;
    }

    // Purement visuel, exécuté uniquement chez le owner
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

    // Instancie le canvas de timer chez le owner uniquement (affichage local)
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
        _uiTimer = -1f;
        _timerText = null;
        _isWaitingRespawn = false;

        if (_canvas != null)
            Destroy(_canvas);

        EnableSpectatorCamera(false);
    }
}