using UnityEngine;

public class PainterController : MonoBehaviour
{

    [SerializeField] private ETeam team = ETeam.BLUE;
    
    [Space]
    [Tooltip("Rayon du pinceau mètres")]
    [SerializeField] private float brushRadius = 0.5f;
    
    [Tooltip("Intervalle minimum (en secondes) entre deux peintures, pour éviter de surcharger le CPU.")]
    [SerializeField] private float paintInterval = 0.05f;

    private Rigidbody _rb;
    private float _paintTimer;

    public ETeam Team
    {
        get => team;
        set => team = value;
    }
    
    public Rigidbody Rigidbody => _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>(); // optionnel, peut être null
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

    // Utilisé si l'un des deux colliders (painter ou plateforme) est marqué "Is Trigger".
    // -> Garde seulement celle des deux méthodes qui correspond à ton setup pour éviter les doublons.
    private void OnTriggerStay(Collider other)
    {
        TryPaint(other);
    }

    private void TryPaint(Collider other)
    {
        if (_paintTimer < paintInterval) return;

        PlatformController platform = other.GetComponent<PlatformController>();
        if (platform == null) return;

        platform.Paint(transform.position, team, brushRadius);
        _paintTimer = 0f;
    }
}