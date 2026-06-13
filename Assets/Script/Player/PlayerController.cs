using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private Renderer _renderer;
    
    private NetworkRigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            _rigidbody = GetComponent<NetworkRigidbody>(); 
            _renderer.material.color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        Move();
    }
    
    private void ConnectAsClient()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    private void Move()
    {
        if (IsOwner)
        {
            //_rigidbody.MovePosition(transform.position + transform.forward * _speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 movement = new Vector3(0,0,1) * _speed * Time.deltaTime;
                _rigidbody.MovePosition(transform.position + movement);
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 movement = new Vector3(0,0,-1) * _speed * Time.deltaTime;
                _rigidbody.MovePosition(transform.position + movement);
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 movement = new Vector3(1,0,0) * _speed * Time.deltaTime;
                _rigidbody.MovePosition(transform.position + movement);
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 movement = new Vector3(-1,0,0) * _speed * Time.deltaTime;
                _rigidbody.MovePosition(transform.position + movement);
            }
        }
    }

    private void Jump()
    {
    
    }
}
