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
    
    private Rigidbody _rb;
    private Vector3 _ogLocation;
    private Quaternion _ogRotation;
    
    // Timer "physique" : n'existe et n'agit que côté serveur (repositionnement du Rigidbody, comme CarController)
    private float _timer = -1f;
    
    // Timer "visuel" : copie locale utilisée uniquement par le owner pour l'affichage du texte
    private float _uiTimer = -1f;
    
    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();
        _ogLocation = _rb.position;
        _ogRotation = _rb.rotation;
    }


    private void Update()
    {
        // Seul le serveur a l'autorité sur le Rigidbody désormais -> seul lui fait le vrai repositionnement
        if (IsServer)
            TimerRespawn();

        // Le owner gère en parallèle sa propre copie du timer, uniquement pour l'affichage
        if (IsOwner)
            TimerUI();
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
        
            _rb.position = _ogLocation;
            _rb.rotation = _ogRotation;
            
            EnablePlayerRpc(true);
            
            return;
        }

        _timer -= Time.deltaTime;
    }

    // Purement visuel, exécuté uniquement chez le owner
    private void TimerUI()
    {
        if (_uiTimer <= -1f) return;

        if (_uiTimer <= 0)
        {
            _uiTimer = -1;
            
            if (_canvas != null)
                Destroy(_canvas);
            
            return;
        }

        if (_timerText != null)
            _timerText.text = _uiTimer.ToString("0.0");
        
        _uiTimer -= Time.deltaTime;
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnablePlayerRpc(bool enable)
    {
        GetComponent<Collider>().enabled = enable;
        GetComponent<Rigidbody>().useGravity = enable;
        _renderer.SetActive(enable);
        GetComponent<CarController>().enabled = enable;
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
    }
}