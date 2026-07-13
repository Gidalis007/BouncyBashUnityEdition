using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
#region Serialized Fields
    [SerializeField] private bool _playable = true;

    [Header("Teams")] 
    [SerializeField] private ETeam _myTeam = ETeam.NONE;
    
    [Space]
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
    
#endregion Serialized Fields
    
#region Attributes
    private Rigidbody _rigidbody;
    private Collider _playerCollider;
    private bool _stopMaxSpeed;
    
    private float _verticalInput;
    
    private float _airPenaltyMemory;
    
    private bool _canBeBump = true;
    private float _delayBeBump = .3f;
    private bool _canSpeed = true;
    
    #endregion Attributes

#region Properties

public Rigidbody Rigidbody
{
    get => _rigidbody;
    set => _rigidbody = value;
}

public bool CanBeBump
{
    get => _canBeBump;
    set
    {
        if (value == true)
            Debug.LogWarning("CanBeBump can't be set to true");
        else
        {
            _canBeBump = value;
            StartCoroutine(DelayCanBump());
        } 
    }
}


public float MaxSpeed
{
    get => _maxSpeed;
    set => _maxSpeed = value;
}

public ETeam MyTeam
{
    get => _myTeam;
    set => _myTeam = value;
}

#endregion Properties

#region Mono

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        if (_playable)
        {


            if (!Physics.Raycast(transform.position, Vector3.down, 1.1f))
            {
                Rigidbody.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);
                _airPenaltyMemory = _airPenality;
            }
            else
            {
                _airPenaltyMemory = 1;
            }

            Move();
            Direction();
        }
    }

    private void Update()
    {

        if (_playable)
        {
            Jump();
            Dash();
        }
    }

    #endregion Mono

#region Methodes
    private void Direction()
    {
        float steerInput = Input.GetAxisRaw("Horizontal");

        // Rotation proportionnelle à la vitesse, plafonnée
        float speedFactor = Mathf.Clamp01(Rigidbody.linearVelocity.magnitude / MaxSpeed);

        // Courbe : tourne bien dès le départ, légèrement moins vite à haute vitesse
        float rotationFactor = Mathf.Lerp(0.5f, 1f, speedFactor);

        float rotationAmount = steerInput * _steerAngle * rotationFactor * Time.fixedDeltaTime * 60f;

        Rigidbody.MoveRotation(
            Rigidbody.rotation *
            Quaternion.Euler(0, rotationAmount, 0)
        );
    }

    private void Move()
    {
        if (_canSpeed)
        {
            _verticalInput = Input.GetAxis("Vertical");

            // Accélération
            Vector3 acceleration = transform.forward * _moveSpeed * _verticalInput * Time.fixedDeltaTime;
            Rigidbody.AddForce(acceleration, ForceMode.VelocityChange);

            // Limitation vitesse (horizontal uniquement)
            Vector3 flatVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
            if (flatVelocity.magnitude > MaxSpeed)
            {
                Vector3 capped = flatVelocity.normalized * MaxSpeed;
                Rigidbody.linearVelocity = new Vector3(capped.x, Rigidbody.linearVelocity.y, capped.z);
            }

            // Traction
            flatVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
            float tractionForce = 1f - Mathf.Clamp01(_traction * Time.fixedDeltaTime * 10f);
            Vector3 forward = transform.forward * Vector3.Dot(flatVelocity, transform.forward);
            Vector3 right   = transform.right   * Vector3.Dot(flatVelocity, transform.right);
            Vector3 tractedVelocity = forward + right * tractionForce;
            Rigidbody.linearVelocity = new Vector3(tractedVelocity.x, Rigidbody.linearVelocity.y, tractedVelocity.z);
        }

        // Drag
        Vector3 draggedVelocity = new Vector3(
            Rigidbody.linearVelocity.x * _drag,
            Rigidbody.linearVelocity.y,
            Rigidbody.linearVelocity.z * _drag
        );
        Rigidbody.linearVelocity = draggedVelocity;
    }

    private void Jump()
    {
        bool canJump = Physics.Raycast(  transform.position, Vector3.down, 1.1f);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void Dash()
    {
        Vector3 dashDir = Vector3.zero;
        
        if (Input.GetKey(KeyCode.UpArrow)) 
            dashDir = transform.forward;
        else if (Input.GetKey(KeyCode.DownArrow))
            dashDir = -transform.forward;
        if ( Input.GetKeyDown(KeyCode.LeftArrow))
            dashDir = -transform.right;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            dashDir = transform.right;

        if (dashDir != Vector3.zero)
        {
            StopCoroutine(StopMaxSpeed());
            StartCoroutine(StopMaxSpeed());
            Vector3 dashVelocity = (dashDir * _dashForce) / _airPenaltyMemory;

            Rigidbody.linearVelocity = new Vector3(
                dashVelocity.x,
                Rigidbody.linearVelocity.y, 
                dashVelocity.z
            );
        }
    }
#endregion Methodes

#region  Coroutines
    private IEnumerator StopMaxSpeed()
    {

        MaxSpeed = MaxSpeed * 2f;

            
        yield return new WaitForSeconds(0.5f);
        

        MaxSpeed = MaxSpeed /2 ;

        StopCoroutine(StopMaxSpeed());
    }

    public IEnumerator StopSpeed(float time)
    {
        _canSpeed = false;
        yield return new WaitForSeconds(time);
        _canSpeed = true;
    }

    private IEnumerator DelayCanBump()
    {
        yield return new WaitForSeconds(_delayBeBump);
        _canBeBump = true;
    }
#endregion Courtines
    
}