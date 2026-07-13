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

    private void OnTriggerEnter(Collider other)
    {
        Bump(other.gameObject);

    }
    

    private void Bump(GameObject bump)
    {
        bool isPushableTag = Array.Exists(_pushableTags, tag => tag == bump.tag);

        if (!isPushableTag) return;
        
        _canPush = false;
        
        Rigidbody targetRb = bump.GetComponent<Rigidbody>();
        if (targetRb == null) return;

        Vector3 pushDir = new Vector3(
            _myCarController.Rigidbody.linearVelocity.x,
            targetRb.linearVelocity.y + _jumpBumpForce,
            _myCarController.Rigidbody.linearVelocity.z
        );
        
        targetRb.AddForce(pushDir, ForceMode.Impulse);

        if (bump.GetComponent<PainterController>())
        {
            bump.GetComponent<PainterController>().Team = _myCarController.MyTeam;
        }
    }
}
