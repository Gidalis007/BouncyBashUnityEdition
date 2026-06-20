using System;
using System.Collections;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Mouvement Base")]
    [SerializeField] private float _moveSpeed = 50f;
    [SerializeField] private float _maxSpeed = 15f;
    [SerializeField] private float _drag = 0.98f;
    [SerializeField] private float _steerAngle = 20f;
    [SerializeField] private float _traction = 1f;
    
    [Space]
    [Header("Mouvement Special")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _dashForce = 30f;
    [SerializeField] private float _airPenality = 20f;


    [Space]
    [Header("Gravity")]
    [SerializeField] float _extraGravity = 20f;
    
    private Vector3 _moveForce;
    private Rigidbody _rigidbody;
    private bool _stopMaxSpeed;
    private float _verticalInput;
    private float _airPenaltyMemory;

    #region Mono

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        if(!Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            _rigidbody.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);
            _airPenaltyMemory = _airPenality;
        }
        else
        {
            _airPenaltyMemory = 1;
        }
        Move();
        Direction();
    }

    private void Update()
    {
        Jump();
        Dash();
    }
    #endregion

    private void Direction()
    {
        float steerInput = Input.GetAxisRaw("Horizontal");

        // Rotation proportionnelle à la vitesse, plafonnée
        float speedFactor = Mathf.Clamp01(_rigidbody.linearVelocity.magnitude / _maxSpeed);

        // Courbe : tourne bien dès le départ, légèrement moins vite à haute vitesse
        float rotationFactor = Mathf.Lerp(0.5f, 1f, speedFactor);

        float rotationAmount = steerInput * _steerAngle * rotationFactor * Time.fixedDeltaTime * 60f;

        _rigidbody.MoveRotation(
            _rigidbody.rotation *
            Quaternion.Euler(0, rotationAmount, 0)
        );
    }

    private void Move()
    {
        _verticalInput = Input.GetAxis("Vertical");

        // Accélération
        _moveForce += transform.forward * _moveSpeed * _verticalInput * Time.fixedDeltaTime;

        // Limitation vitesse
        if (!_stopMaxSpeed)
            _moveForce = Vector3.ClampMagnitude(_moveForce, _maxSpeed);

        // Traction : réaligne la vélocité vers le forward du véhicule
        // _traction = 1 → colle bien (pas de drift)
        // _traction = 0 → glisse librement
        float tractionForce = 1f - Mathf.Clamp01(_traction * Time.fixedDeltaTime * 10f);
        Vector3 forward = transform.forward * Vector3.Dot(_moveForce, transform.forward);
        Vector3 right   = transform.right   * Vector3.Dot(_moveForce, transform.right);
        _moveForce = forward + right * tractionForce;

        // Drag
        _moveForce *= _drag;

        // Application
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

        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.UpArrow))
            dashDir = transform.forward;
        else if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.DownArrow))
            dashDir = -transform.forward;
        else if (Input.GetKeyDown(KeyCode.J)  || Input.GetKeyDown(KeyCode.LeftArrow))
            dashDir = -transform.right;
        else if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.RightArrow))
            dashDir = transform.right;
        
        if (dashDir != Vector3.zero)
        {
            StopCoroutine(StopMaxSpeed());
            StartCoroutine(StopMaxSpeed());
            _moveForce = (dashDir * _dashForce)/_airPenaltyMemory;
        }
    }

    private IEnumerator StopMaxSpeed()
    {
        _stopMaxSpeed = true;
        Debug.Log("j'annule la limitation");
            
        yield return new WaitForSeconds(1f);
        
        _stopMaxSpeed = false;
        Debug.Log("je remais normal");
        StopCoroutine(StopMaxSpeed());
    }
}