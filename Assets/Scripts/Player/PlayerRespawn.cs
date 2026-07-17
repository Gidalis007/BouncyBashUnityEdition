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
    private GameObject _canvas;
    private TextMeshProUGUI _timerText;
    
    private Rigidbody _rb;
    private Vector3 _ogLocation;
    private Quaternion _ogRotation;
    
    private float _timer = -1f;
    
    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();
        _ogLocation = _rb.position;
        _ogRotation = _rb.rotation;
    }


    private void Update()
    {
        TimerRespawn();
    }


    public void Respawn(float time)
    {
        if (!IsOwner) return;

        _canvas = Instantiate(_canvasPrefab, Vector3.zero, Quaternion.identity, transform);
        GetComponent<MeshRenderer>().enabled = false;
        
        for (int i = 0; i < _canvas.GetComponentsInChildren<TextMeshProUGUI>().Length; i++)
        {
            if (_canvas.GetComponentsInChildren<TextMeshProUGUI>()[i].CompareTag("Timer")) 
                _timerText = _canvas.GetComponentsInChildren<TextMeshProUGUI>()[i];
        }
        
        EnablePlayerRpc(false);
        
        _timer = time;
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
            
            Destroy(_canvas);
            
            EnablePlayerRpc(true);
            
            return;
        }

        _timerText.text = _timer.ToString("0.0");
        _timer -= Time.deltaTime;
    }
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnablePlayerRpc(bool enable)
    {
        GetComponent<Collider>().enabled = enable;
        GetComponent<Rigidbody>().useGravity = enable;
        GetComponent<MeshRenderer>().enabled = enable;
        GetComponent<CarController>().enabled = enable;
    }


}
