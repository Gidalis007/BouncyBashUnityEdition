using System;
using System.Collections;
using UnityEngine;

public class ForwardColliderController : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private float _bumpForce = 1.2f;
    [SerializeField] private float _jumpBumpForce = 5f;
    [SerializeField] private float _minPushForce = 5f;
    [SerializeField] private string[] _pushableTags = { "Brush", "Player" };

    [SerializeField] private float _maxPushSecond = 0.1f;

    #endregion SerializeFields

    #region Attributes

    private bool _canPush = true;
    private Collider _forwardCollider;
    private CarController _myCarController;

    #endregion Attributes

    private void Awake()
    {
        _forwardCollider = GetComponent<Collider>();
        _myCarController = gameObject.GetComponentInParent<CarController>();
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(Bump(other.gameObject));
    }

    private IEnumerator Bump(GameObject bump)
    {
        bool isPushableTag = Array.Exists(_pushableTags, tag => tag == bump.tag);

        if (!isPushableTag || !_canPush) yield break;
        
        _canPush = false;
        
        Rigidbody targetRb = bump.GetComponent<Rigidbody>();
        if (targetRb == null) yield break;

        Vector3 pushDir = new Vector3(
            _myCarController.Rigidbody.linearVelocity.x,
            0f,
            _myCarController.Rigidbody.linearVelocity.z
        );

        if (pushDir.magnitude < _minPushForce)
            pushDir = pushDir.magnitude > 0.01f
                ? pushDir.normalized * _minPushForce
                : _myCarController.transform.forward * _minPushForce;

        targetRb.AddForce(pushDir, ForceMode.Impulse);
        
        yield return new WaitForSeconds(_maxPushSecond);
        _canPush = true;
    }
}
