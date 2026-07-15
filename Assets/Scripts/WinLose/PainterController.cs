using Unity.Netcode;
using UnityEngine;

public class PainterController : NetworkBehaviour
{

    [SerializeField] private NetworkVariable<ETeam> _team = new NetworkVariable<ETeam>(ETeam.BLUE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [Space]
    [Tooltip("Rayon du pinceau mètres")]
    [SerializeField] private float brushRadius = 0.5f;
    
    [Tooltip("Intervalle minimum (en secondes) entre deux peintures, pour éviter de surcharger le CPU.")]
    [SerializeField] private float paintInterval = 0.05f;

    private Rigidbody _rb;
    private float _paintTimer;

    public ETeam Team
    {
        get => _team.Value;
        set
        {
            _team.Value = value;
            GetComponent<Renderer>().material.color = _team.Value == ETeam.BLUE ? Color.blue : Color.red;
        }
    }
    
    public Rigidbody Rigidbody => _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //HandleMovementInput();
        _paintTimer += Time.deltaTime;
    }
    
    private void OnCollisionStay(Collision collision)
    {
        TryPaint(collision.collider);
    }
    
    private void OnTriggerStay(Collider other)
    {
        TryPaint(other);
    }

    private void TryPaint(Collider other)
    {
        if (_paintTimer < paintInterval) return;

        PlatformController platform = other.GetComponent<PlatformController>();
        if (platform == null) return;

        platform.Paint(transform.position, Team, brushRadius);
        _paintTimer = 0f;
    }
}