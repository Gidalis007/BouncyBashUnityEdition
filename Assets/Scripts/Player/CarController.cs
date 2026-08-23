using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class CarController : NetworkBehaviour
{
#region Serialized Fields
    [SerializeField] private bool _playable = true;

    [Header("Teams")] 
    [SerializeField] private NetworkVariable<ETeam>  _myTeam = new NetworkVariable<ETeam>(ETeam.NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
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
    
    // Inputs reçus (sur le serveur) ou capturés localement (sur le owner) chaque FixedUpdate
    private float _verticalInput;
    private float _horizontalInput;
    
    // N'est mis à jour et utilisé que côté serveur, qui est seul à exécuter la physique
    private float _airPenaltyMemory = 1f;
    
    private bool _canBeBump = true;
    private float _delayBeBump = .3f;
    private bool _canSpeed = true;

    // Référence à la coroutine de boost du Dash, pour pouvoir vraiment l'arrêter (cf. bug fix)
    private Coroutine _stopMaxSpeedRoutine;
    
    private Camera _camera;
    
    
    #endregion Attributes
    
#region Network Attribute & SerializedFields
    [SerializeField] private NetworkVariable<float> _testSerialize =  new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> _test =  new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    #endregion Network Attribute & SerializedFields

#region Types

    // Direction du dash envoyée par le client : le serveur reconvertit lui-même
    // cette intention en vecteur monde à partir de SON propre transform (autorité).
    private enum EDashDirection
    {
        None,
        Forward,
        Backward,
        Left,
        Right
    }

#endregion Types

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
    get => _myTeam.Value;
    set => _myTeam.Value = value;
}

#endregion Properties

#region Mono

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        
    }

    public override void OnNetworkSpawn()
    {
        _camera = GetComponentInChildren<Camera>();

        if (!IsOwner)
        {
            _camera.enabled = false;
        }

        // Filet de sécurité : seul le serveur simule la physique (Rigidbody non-kinematic).
        // NetworkRigidbody gère normalement déjà ce switch automatiquement selon l'autorité,
        // cette ligne garantit juste le bon état dès le premier frame.
        Rigidbody.isKinematic = !IsServer;
        
        Debug.Log("OnNetworkSpawn");
        
        GameManager.Instance.PlayersController.Add(new NetworkObjectReference(this.NetworkObject));
        
        base.OnNetworkSpawn();
    }

    private void FixedUpdate()
    {
        if (!_playable) return;

        // Le owner capture ses inputs de déplacement à chaque tick physique et les envoie au serveur.
        // Si le owner EST le serveur (host), on évite l'aller-retour réseau inutile.
        if (IsOwner)
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            if (IsServer)
            {
                _verticalInput = vertical;
                _horizontalInput = horizontal;
            }
            else
            {
                SendMoveInputServerRpc(vertical, horizontal);
            }
        }

        // Seul le serveur exécute la simulation (gravité, déplacement, rotation).
        if (!IsServer) return;

        if (!Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            Rigidbody.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);
            _airPenaltyMemory = _airPenality;
        }
        else
        {
            _airPenaltyMemory = 1f;
        }

        Move();
        Direction();

    }

    private void Update()
    {
        if (!_playable || !IsOwner) return;

        // Saut : action ponctuelle, envoyée au serveur qui valide et applique la force.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsServer) TryJump();
            else RequestJumpServerRpc();
        }

        // Dash : on envoie l'INTENTION (avant/arrière/gauche/droite), pas un vecteur calculé
        // localement, pour que ce soit le serveur qui fasse foi sur l'orientation de la voiture.
        EDashDirection dashDirection = EDashDirection.None;

        if (Input.GetKey(KeyCode.UpArrow))
            dashDirection = EDashDirection.Forward;
        else if (Input.GetKey(KeyCode.DownArrow))
            dashDirection = EDashDirection.Backward;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            dashDirection = EDashDirection.Left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            dashDirection = EDashDirection.Right;

        if (dashDirection != EDashDirection.None)
        {
            if (IsServer) ExecuteDash(dashDirection);
            else RequestDashServerRpc(dashDirection);
        }
        
        if (Input.GetKeyDown(KeyCode.U))
            DebugSwitchTeam();
    }

    #endregion Mono

#region Methodes
    private void Direction()
    {
        float steerInput = _horizontalInput;

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

    // Exécutée uniquement côté serveur (appelée directement si host, via RPC sinon)
    private void TryJump()
    {
        bool canJump = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (canJump)
        {
            Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    // Exécutée uniquement côté serveur (appelée directement si host, via RPC sinon)
    private void ExecuteDash(EDashDirection direction)
    {
        Vector3 dashDir = direction switch
        {
            EDashDirection.Forward  => transform.forward,
            EDashDirection.Backward => -transform.forward,
            EDashDirection.Left     => -transform.right,
            EDashDirection.Right    => transform.right,
            _ => Vector3.zero
        };

        if (dashDir == Vector3.zero) return;

        if (_stopMaxSpeedRoutine != null)
            StopCoroutine(_stopMaxSpeedRoutine);
        _stopMaxSpeedRoutine = StartCoroutine(StopMaxSpeed());

        // Mathf.Max évite une division par 0 (ou par une valeur non encore initialisée) -> vitesse infinie
        Vector3 dashVelocity = (dashDir * _dashForce) / Mathf.Max(_airPenaltyMemory, 1f);

        Rigidbody.linearVelocity = new Vector3(
            dashVelocity.x,
            Rigidbody.linearVelocity.y, 
            dashVelocity.z
        );
    }
    

    #endregion Methodes

#region Network RPCs

    // Input continu de déplacement (avancer/tourner), envoyé chaque FixedUpdate.
    // Unreliable : la perte occasionnelle d'un paquet n'a aucune conséquence, le suivant arrive juste après.
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendMoveInputServerRpc(float vertical, float horizontal)
    {
        _verticalInput = vertical;
        _horizontalInput = horizontal;
    }

    // Action ponctuelle -> RPC fiable (par défaut) pour ne surtout pas perdre l'événement.
    [ServerRpc]
    private void RequestJumpServerRpc()
    {
        TryJump();
    }

    [ServerRpc]
    private void RequestDashServerRpc(EDashDirection direction)
    {
        ExecuteDash(direction);
    }

#endregion Network RPCs

#region  Coroutines
    private IEnumerator StopMaxSpeed()
    {
        MaxSpeed = MaxSpeed * 2f;

        yield return new WaitForSeconds(0.5f);

        MaxSpeed = MaxSpeed / 2f;
        _stopMaxSpeedRoutine = null;
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

#region Debug

private void DebugSwitchTeam()
{
    if (MyTeam == ETeam.BLUE)
        MyTeam = ETeam.RED;
    else if (MyTeam == ETeam.RED)
        MyTeam = ETeam.BLUE;
    else 
        MyTeam = ETeam.RED;
}

#endregion Debug
}