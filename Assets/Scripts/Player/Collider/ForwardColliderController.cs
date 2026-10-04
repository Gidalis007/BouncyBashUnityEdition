using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class ForwardColliderController : NetworkBehaviour
{
    #region SerializeFields

    [Header("Bump")]
    [SerializeField] private float _bumpForce = 1.2f;
    [SerializeField] private float _minPushForce = 5f;
    [SerializeField] private float _maxPushSecond = 0.1f;
    
    [Header("Jump")]
    [SerializeField] private float _jumpBumpForce = 5f;
    [SerializeField] private float _minJumpPush = 2f;
    [SerializeField] private float _maxJumpPush = 15f;

    
    [Header("Push")]
    [SerializeField] private string[] _pushableTags = { "Brush", "Player" };

    #endregion SerializeFields

    #region Attributes

    private bool _canPush = true;
    private Collider _forwardCollider;
    private CarController _myCarController;

    #endregion Attributes

    #region Properties

    #endregion Properties

    #region Mono

    private void Awake()
    {
        _forwardCollider = GetComponent<Collider>();
        _myCarController = gameObject.GetComponentInParent<CarController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canPush) return;
        Bump(other.gameObject);
    }

    private void Bump(GameObject bump)
    {
        bool isPushableTag = Array.Exists(_pushableTags, tag => tag == bump.tag);
        if (!isPushableTag) return;

        Rigidbody targetRb = bump.GetComponent<Rigidbody>();
        if (targetRb == null) return;

        _canPush = false;
        StartCoroutine(ResetPushCooldown());

        Vector3 originVelocity = new Vector3(
            _myCarController.Rigidbody.linearVelocity.x,
            0f,
            _myCarController.Rigidbody.linearVelocity.z
        );

        float impactSpeed = originVelocity.magnitude;

        Vector3 horizontalPush = originVelocity * _bumpForce;

        if (horizontalPush.magnitude < _minPushForce)
        {
            Vector3 pushDirection = horizontalPush.sqrMagnitude > 0.001f
                ? horizontalPush.normalized
                : _myCarController.transform.forward;

            horizontalPush = pushDirection * _minPushForce;
        }

        // Poussée verticale bornée entre min et max
        float verticalPush = Mathf.Clamp(
            impactSpeed * _jumpBumpForce,
            _minJumpPush,
            _maxJumpPush
        );

        targetRb.AddForce(new Vector3(horizontalPush.x, 0f, horizontalPush.z), ForceMode.Impulse);
        targetRb.AddForce(Vector3.up * verticalPush, ForceMode.VelocityChange);

        PainterController painter = bump.GetComponent<PainterController>();
        if (painter)
        {
            painter.ChangeTeamRpc(_myCarController.MyTeam);
        }

        // Un "pump" réussi sur un joueur rend instantanément le dash au percuteur.
        if (bump.CompareTag("Player"))
        {
            _myCarController.NotifyPlayerBump();
        }
    }

    private IEnumerator ResetPushCooldown()
    {
        yield return new WaitForSeconds(_maxPushSecond);
        _canPush = true;
    }
    

    #endregion Methods
    
}
