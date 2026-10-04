using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class PainterController : NetworkBehaviour
{

    #region SerializedFields

    [SerializeField] private NetworkVariable<ETeam> _team = new NetworkVariable<ETeam>(ETeam.BLUE,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Space]
    [Tooltip("Rayon du pinceau mètres")]
    [SerializeField] private float _brushRadius = 0.5f;

    [Tooltip("Intervalle minimum (en secondes) entre deux peintures, pour éviter de surcharger le CPU.")]
    [SerializeField] private float _paintInterval = 0.05f;

    #endregion SerializedFields

    #region Attributes

    private Rigidbody _rb;
    private float _paintTimer;

    #endregion Attributes

    #region Properties

    public Rigidbody Rigidbody => _rb;

    #endregion Properties

    #region Mono
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
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

    #endregion Mono

    #region Methods

    protected virtual ETeam PaintTeam => _team.Value;
    protected virtual bool CanPaint => true;

    protected virtual bool ServerAuthoritative => false;

    private void TryPaint(Collider other)
    {
        if (!CanPaint || _paintTimer < _paintInterval) return;

        PlatformController platform = other.GetComponent<PlatformController>();
        if (platform == null) return;

        if (ServerAuthoritative && !IsServer) return;

        bool changed = platform.Paint(transform.position, PaintTeam, _brushRadius);
        _paintTimer = 0f;

        if (changed && ServerAuthoritative && IsServer)
        {
            BroadcastStrokeRpc(transform.position, PaintTeam, _brushRadius);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnPlayerTouched(collision);
    }

    protected virtual void OnPlayerTouched(Collision collision)
    {

        if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out NetworkObject networkObject))
            ChangeOwnerRpc(networkObject.OwnerClientId);
    }

    #endregion Methods

    #region Rpc
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeOwnerRpc(ulong ownerClientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(ownerClientId);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void BroadcastStrokeRpc(Vector3 worldPosition, ETeam team, float radius)
    {
        foreach (PlatformController platform in FindObjectsByType<PlatformController>())
        {
            if (platform != null)
            {
                platform.Paint(worldPosition, team, radius);
            }
        }
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeTeamRpc(ETeam team)
    {
        _team.Value = team;
        ChangeColorRpc(team == ETeam.BLUE ? Color.blue :  team == ETeam.RED ? Color.red : Color.black);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeColorRpc(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }
    #endregion Rpc

}
