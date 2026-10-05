using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
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
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _dashForce = 30f;
    [SerializeField] private float _dashCooldown = 3f;
    [SerializeField] private float _airPenality = 20f;


    [Space]
    [Header("Gravity")]
    [SerializeField] float _extraGravity = 20f;

    [Space]
    [Header("Dash Indicator")]
    [SerializeField] private DashIndicator _dashIndicator;

    [Space]
    [Header("Kill Reward")]
    [SerializeField] private CarPainter _carPainter;

    [Tooltip("Délai max entre un pump et la mort pour créditer le kill (secondes).")]
    [SerializeField] private float _killCreditWindow = 6f;

#endregion Serialized Fields

#region Attributes
    private Rigidbody _rigidbody;
    private Collider _playerCollider;
    private bool _stopMaxSpeed;

    private float _verticalInput;
    private float _horizontalInput;

    private float _airPenaltyMemory = 1f;

    private bool _canBeBump = true;
    private float _delayBeBump = .3f;
    private bool _canSpeed = true;

    private bool _canDash = true;
    private Coroutine _dashCooldownRoutine;

    private NetworkVariable<bool> _dashReady = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> _killCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private CarController _lastBumper;
    private float _lastBumpTime = -1000f;
    private Coroutine _carPainterRoutine;

    private float _coyoteTimer;

    private bool _wasGrounded = true;
    private float _lastVerticalSpeed;
    private bool _engineStarted;

    private Coroutine _stopMaxSpeedRoutine;
    private float _baseMaxSpeed;

    private Camera _camera;
    private Renderer[] _teamRenderers;


    #endregion Attributes

#region Network Attribute & SerializedFields
    [SerializeField] private NetworkVariable<float> _testSerialize =  new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> _test =  new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    #endregion Network Attribute & SerializedFields

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

public bool IsTeamOne
{
    get => MyTeam == ETeam.RED;
}

public bool IsTeamTwo
{
    get => MyTeam == ETeam.BLUE;
}

public bool IsDashReady
{
    get => IsServer ? _canDash : _dashReady.Value;
}

public int KillCount => _killCount.Value;

#endregion Properties

#region Mono

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        _baseMaxSpeed = MaxSpeed;

    }

    public override void OnNetworkSpawn()
    {
        _camera = GetComponentInChildren<Camera>();

        if (!IsOwner)
        {
            _camera.enabled = false;
        }

        _teamRenderers = GetComponentsInChildren<Renderer>();

        if (IsOwner)
            MyTeam = OwnerClientId == 0 ? ETeam.RED : ETeam.BLUE;

        _myTeam.OnValueChanged += OnTeamChanged;
        ApplyTeamColor(MyTeam);

        _dashReady.OnValueChanged += OnDashReadyChanged;
        UpdateDashIndicator(_dashReady.Value);

        Rigidbody.isKinematic = !IsServer;

        if (IsOwner)
        {
            NetworkTransform ownerTransform = GetComponent<NetworkTransform>();

            if (ownerTransform != null)
            {
                ownerTransform.Interpolate = false;
            }
        }
        
        GameManager.Instance.PlayersController.Add(new NetworkObjectReference(this.NetworkObject));

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (_engineStarted && AudioManager.Instance != null)
        {
            AudioManager.Instance.EngineStop();
            _engineStarted = false;
        }

        _myTeam.OnValueChanged -= OnTeamChanged;
        _dashReady.OnValueChanged -= OnDashReadyChanged;

        base.OnNetworkDespawn();
    }

    private void OnDisable()
    {
        if (_engineStarted && AudioManager.Instance != null)
        {
            AudioManager.Instance.EngineStop();
            _engineStarted = false;
        }
    }

    private void FixedUpdate()
    {
        if (!_playable) return;

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

        if (!IsServer) return;

        bool grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (!grounded)
        {
            Rigidbody.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);
            _airPenaltyMemory = _airPenality;
            _coyoteTimer -= Time.fixedDeltaTime;
            _wasGrounded = false;
        }
        else
        {
            _airPenaltyMemory = 1f;
            _coyoteTimer = _coyoteTime;

            if (!_wasGrounded && _lastVerticalSpeed < -8f)
            {
                PlayLandRpc(transform.position);
            }

            _wasGrounded = true;
        }

        _lastVerticalSpeed = Rigidbody.linearVelocity.y;

        Move();
        Direction();

    }

    private void Update()
    {
        if (!_playable || !IsOwner) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EngineStart();
            AudioManager.Instance.SetEngine(
                Rigidbody != null ? Mathf.Clamp01(Rigidbody.linearVelocity.magnitude / Mathf.Max(MaxSpeed, 0.01f)) : 0f,
                Mathf.Abs(_verticalInput));
            _engineStarted = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsServer) TryJump();
            else RequestJumpServerRpc();
        }

        EDashDirection dashDirection = EDashDirection.NONE;

        if (Input.GetKey(KeyCode.UpArrow))
            dashDirection = EDashDirection.FORWARD;
        else if (Input.GetKey(KeyCode.DownArrow))
            dashDirection = EDashDirection.BACKWARD;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            dashDirection = EDashDirection.LEFT;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            dashDirection = EDashDirection.RIGHT;

        if (dashDirection != EDashDirection.NONE)
        {
            if (IsServer) ExecuteDash(dashDirection);
            else RequestDashServerRpc(dashDirection);
        }

        if (Input.GetKeyDown(KeyCode.T))
            DebugSwitchTeam();
    }

    #endregion Mono

#region Methodes
    private void Direction()
    {
        float steerInput = _horizontalInput;

        float speedFactor = Mathf.Clamp01(Rigidbody.linearVelocity.magnitude / MaxSpeed);

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
            Vector3 acceleration = transform.forward * _moveSpeed * _verticalInput * Time.fixedDeltaTime;
            Rigidbody.AddForce(acceleration, ForceMode.VelocityChange);

            Vector3 flatVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
            if (flatVelocity.magnitude > MaxSpeed)
            {
                Vector3 capped = flatVelocity.normalized * MaxSpeed;
                Rigidbody.linearVelocity = new Vector3(capped.x, Rigidbody.linearVelocity.y, capped.z);
            }

            flatVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
            float tractionForce = 1f - Mathf.Clamp01(_traction * Time.fixedDeltaTime * 10f);
            Vector3 forward = transform.forward * Vector3.Dot(flatVelocity, transform.forward);
            Vector3 right   = transform.right   * Vector3.Dot(flatVelocity, transform.right);
            Vector3 tractedVelocity = forward + right * tractionForce;
            Rigidbody.linearVelocity = new Vector3(tractedVelocity.x, Rigidbody.linearVelocity.y, tractedVelocity.z);
        }

        Vector3 draggedVelocity = new Vector3(
            Rigidbody.linearVelocity.x * _drag,
            Rigidbody.linearVelocity.y,
            Rigidbody.linearVelocity.z * _drag
        );
        Rigidbody.linearVelocity = draggedVelocity;
    }

    private void TryJump()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        bool canJump = grounded || _coyoteTimer > 0f;

        if (canJump)
        {
            _coyoteTimer = 0f;
            Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            PlayJumpRpc(transform.position);
        }
    }

    private void ExecuteDash(EDashDirection direction)
    {
        if (!_canDash) return;

        Vector3 dashDir = direction switch
        {
            EDashDirection.FORWARD => transform.forward,
            EDashDirection.BACKWARD => -transform.forward,
            EDashDirection.LEFT => -transform.right,
            EDashDirection.RIGHT => transform.right,
            EDashDirection.NONE => Vector3.zero
        };

        if (dashDir == Vector3.zero) return;

        if (_stopMaxSpeedRoutine != null)
            StopCoroutine(_stopMaxSpeedRoutine);
        _stopMaxSpeedRoutine = StartCoroutine(StopMaxSpeed());

        Vector3 dashVelocity = (dashDir * _dashForce) / Mathf.Max(_airPenaltyMemory, 1f);

        Rigidbody.linearVelocity = new Vector3(
            dashVelocity.x,
            Rigidbody.linearVelocity.y,
            dashVelocity.z
        );

        _canDash = false;
        _dashReady.Value = false;
        UpdateDashIndicator(false);
        PlayDashRpc(transform.position);
        if (_dashCooldownRoutine != null)
            StopCoroutine(_dashCooldownRoutine);
        _dashCooldownRoutine = StartCoroutine(DashCooldownRoutine());
    }


    #endregion Methodes

#region Dash Indicator

    private void OnDashReadyChanged(bool previousReady, bool newReady)
    {
        UpdateDashIndicator(newReady);
    }

    private void UpdateDashIndicator(bool ready)
    {
        if (_dashIndicator != null)
            _dashIndicator.SetAvailable(ready);
    }

    public void NotifyPlayerBump()
    {
        if (IsServer)
            RestoreDash();
        else
            RequestRestoreDashServerRpc();
    }

    private void RestoreDash()
    {
        if (!IsServer || _canDash) return;

        if (_dashCooldownRoutine != null)
        {
            StopCoroutine(_dashCooldownRoutine);
            _dashCooldownRoutine = null;
        }

        _canDash = true;
        _dashReady.Value = true;
        UpdateDashIndicator(true);
    }

#endregion Dash Indicator

#region Kill Reward

    public void RegisterBumpFrom(CarController bumper)
    {
        if (bumper == null || bumper == this)
        {
            return;
        }

        _lastBumper = bumper;
        _lastBumpTime = Time.time;
    }

    public CarController GetKillCreditor()
    {
        if (!IsServer)
        {
            return null;
        }

        if (_lastBumper == null || _lastBumper == this)
        {
            return null;
        }

        if (Time.time - _lastBumpTime > _killCreditWindow)
        {
            return null;
        }

        return _lastBumper;
    }

    public bool RegisterKill()
    {
        if (!IsServer)
        {
            return false;
        }

        int next = _killCount.Value + 1;

        if (next >= 3)
        {
            _killCount.Value = 0;
            return true;
        }

        _killCount.Value = next;
        return false;
    }

    public void EnableCarPainter(float duration, ETeam team)
    {
        if (!IsServer)
        {
            return;
        }

        if (_carPainterRoutine != null)
        {
            StopCoroutine(_carPainterRoutine);
        }

        _carPainterRoutine = StartCoroutine(CarPainterRoutine(duration, team));
    }

    private IEnumerator CarPainterRoutine(float duration, ETeam team)
    {
        ApplyCarPainterLocal(true, team);
        SetCarPainterRpc(true, team);

        yield return new WaitForSeconds(duration);

        ApplyCarPainterLocal(false, ETeam.NONE);
        SetCarPainterRpc(false, ETeam.NONE);

        _carPainterRoutine = null;
    }

    private void ApplyCarPainterLocal(bool painting, ETeam team)
    {
        if (_carPainter != null)
        {
            _carPainter.SetPainting(painting, team);
        }
    }

#endregion Kill Reward

#region Team

    private void OnTeamChanged(ETeam previousTeam, ETeam newTeam)
    {
        ApplyTeamColor(newTeam);
    }

    private void ApplyTeamColor(ETeam team)
    {
        if (team != ETeam.RED && team != ETeam.BLUE)
            return;

        Color teamColor = team == ETeam.RED ? Color.red : Color.blue;

        if (_teamRenderers == null)
            return;

        foreach (Renderer teamRenderer in _teamRenderers)
        {
            if (teamRenderer != null)
                teamRenderer.material.color = teamColor;
        }
    }

#endregion Team

#region Network RPCs

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void SendMoveInputServerRpc(float vertical, float horizontal)
    {
        _verticalInput = vertical;
        _horizontalInput = horizontal;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void RequestJumpServerRpc()
    {
        TryJump();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    private void RequestDashServerRpc(EDashDirection direction)
    {
        ExecuteDash(direction);
    }

    [Rpc(SendTo.Server)]
    private void RequestRestoreDashServerRpc()
    {
        RestoreDash();
    }

    [Rpc(SendTo.Everyone)]
    private void SetCarPainterRpc(bool painting, ETeam team)
    {
        ApplyCarPainterLocal(painting, team);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDashRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAt("dash_whoosh", position);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayJumpRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAt("jump", position);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayLandRpc(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLand(position);
        }
    }

#endregion Network RPCs

#region  Coroutines
    private IEnumerator StopMaxSpeed()
    {
        MaxSpeed = _baseMaxSpeed * 2f;

        yield return new WaitForSeconds(0.5f);

        MaxSpeed = _baseMaxSpeed;
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

    private IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
        _dashReady.Value = true;
        UpdateDashIndicator(true);
        _dashCooldownRoutine = null;
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
