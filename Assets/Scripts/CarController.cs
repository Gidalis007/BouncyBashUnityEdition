using System.Collections;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 50f;
    [SerializeField] private float _maxSpeed = 15f;
    [SerializeField] private float _drag = 0.98f;
    [SerializeField] private float _steerAngle = 20f;
    [SerializeField] private float _traction = 1f;

    [Space]

    public float _extraGravity = 20f;

    [Space]

    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _dashForce = 30f;


    private Vector3 _moveForce;
    private Rigidbody _rigidbody;
    private bool _stopMaxSpeed = false;

    #region Mono
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(!Physics.Raycast(transform.position, Vector3.down, 1.1f))
            _rigidbody.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);
        Move();
        Direction();
    }

    private void Update()
    {
        Jump();
        Dash();

        Debug.Log(_moveForce);
    }
    #endregion

    private void Direction()
    {
        float steerInput = Input.GetAxis("Horizontal");

        if (_rigidbody.linearVelocity.magnitude > 0.1f)
        {
            float rotationAmount =
                steerInput *
                _steerAngle *
                (_rigidbody.linearVelocity.magnitude / _maxSpeed);

            _rigidbody.MoveRotation(
                _rigidbody.rotation *
                Quaternion.Euler(0, rotationAmount, 0)
            );
        }
    }

    private void Move()
    {
        // Acc�l�ration
        float verticalInput = Input.GetAxis("Vertical");
        _moveForce += transform.forward * _moveSpeed * verticalInput * Time.fixedDeltaTime;

        // Limitation vitesse
        if (!_stopMaxSpeed)
            _moveForce = Vector3.ClampMagnitude(_moveForce, _maxSpeed);

        // Traction
        _moveForce = Vector3.Lerp(
            _moveForce.normalized,
            transform.forward,
            _traction * Time.fixedDeltaTime
        ) * _moveForce.magnitude;

        // Drag
        _moveForce *= _drag;

        // Application de la vitesse au Rigidbody
        _rigidbody.linearVelocity = new Vector3(
            _moveForce.x,
            _rigidbody.linearVelocity.y,
            _moveForce.z
        );
    }

    private void Jump()
    {
        bool canJump = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void Dash()
    {
        
        Vector3 dashDir = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.I))
            dashDir = transform.forward;
        else if (Input.GetKeyDown(KeyCode.K))
            dashDir = -transform.forward;
        else if (Input.GetKeyDown(KeyCode.J))
            dashDir = -transform.right;
        else if (Input.GetKeyDown(KeyCode.L))
            dashDir = transform.right;

        if (dashDir != Vector3.zero)
        {
            //StartCoroutine(StopMaxSpeed());
            _moveForce = dashDir * _dashForce;
        }
    }

    private IEnumerator StopMaxSpeed()
    {
        _stopMaxSpeed = true;
        Debug.Log("j'annule la limitation");
            
        yield return new WaitForSeconds(5f);
        
        _stopMaxSpeed = false;
        Debug.Log("je remais normal");
        StopCoroutine(StopMaxSpeed());
    }
}