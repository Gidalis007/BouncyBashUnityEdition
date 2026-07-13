using UnityEngine;

public class PainterController : MonoBehaviour
{

    [SerializeField] private ETeam _team = ETeam.BLUE;
    
    [Space]
    [Tooltip("Rayon du pinceau mètres")]
    [SerializeField] private float brushRadius = 0.5f;
    
    [Tooltip("Intervalle minimum (en secondes) entre deux peintures, pour éviter de surcharger le CPU.")]
    [SerializeField] private float paintInterval = 0.05f;

    private Rigidbody _rb;
    private float _paintTimer;

    public ETeam Team
    {
        get => _team;
        set
        {
            _team = value;
            GetComponent<Renderer>().material.color = _team == ETeam.BLUE ? Color.blue : Color.red;
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

        platform.Paint(transform.position, _team, brushRadius);
        _paintTimer = 0f;
    }
}