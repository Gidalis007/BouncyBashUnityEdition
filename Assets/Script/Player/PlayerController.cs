using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    
    private Rigidbody _rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            _rigidbody = GetComponent<Rigidbody>(); 
            GetComponent<Renderer>().material.color = Color.red;
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
            _rigidbody.MovePosition(transform.position + transform.forward * _speed * Time.deltaTime);
        }
    }
}
