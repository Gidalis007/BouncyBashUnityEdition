using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private float _kickMultiplier = 1.5f;
    [SerializeField] private float _minCarSpeedToKick = 2f;
    [SerializeField] private float _maxKickSecond = 0.05f;

    #endregion SerializeFields

    #region Attributes

    private bool _canKick = true;
    private Rigidbody _rb;

    #endregion Attributes

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_canKick) return;

        CarController car = other.GetComponentInParent<CarController>();
        if (car == null) return;

        Vector3 carVelocity = car.Rigidbody.linearVelocity;
        carVelocity.y = 0f;
        float carSpeed = carVelocity.magnitude;
        if (carSpeed < _minCarSpeedToKick) return;
        
        Debug.Log("Ball Kicked");

        Vector3 contactDir = transform.position - other.transform.position;
        contactDir.y = 0f;
        contactDir.Normalize();

        Vector3 kickDir = (contactDir * 0.5f + carVelocity.normalized * 0.5f).normalized;

        _rb.AddForce(kickDir * carSpeed * _kickMultiplier, ForceMode.Impulse);


        _canKick = false;
        Invoke(nameof(ResetKick), _maxKickSecond);
    }

    private void ResetKick()
    {
        _canKick = true;
    }
}