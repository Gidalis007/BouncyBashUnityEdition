using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 50f;
    [SerializeField] private float _maxSpeed = 15f;
    [SerializeField] private float _drag = 0.98f;
    [SerializeField] private float _steerAngle = 20f;
    [SerializeField] private float _traction = 1f;

    private Vector3 _moveForce;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Accélération
        float verticalInput = Input.GetAxis("Vertical");
        _moveForce += transform.forward * _moveSpeed * verticalInput * Time.fixedDeltaTime;

        // Limitation vitesse
        _moveForce = Vector3.ClampMagnitude(_moveForce, _maxSpeed);

        // Traction
        _moveForce = Vector3.Lerp(
            _moveForce.normalized,
            transform.forward,
            _traction * Time.fixedDeltaTime
        ) * _moveForce.magnitude;

        // Drag
        _moveForce *= _drag;

        // Application de la vitesse au Rigidbody
        _rigidbody.linearVelocity = new Vector3(
            _moveForce.x,
            _rigidbody.linearVelocity.y,
            _moveForce.z
        );

        // Direction
        float steerInput = Input.GetAxis("Horizontal");

        if (_rigidbody.linearVelocity.magnitude > 0.1f)
        {
            float rotationAmount =
                steerInput *
                _steerAngle *
                (_rigidbody.linearVelocity.magnitude / _maxSpeed);

            _rigidbody.MoveRotation(
                _rigidbody.rotation *
                Quaternion.Euler(0, rotationAmount, 0)
            );
        }

        Debug.DrawRay(transform.position, _moveForce.normalized * 3, Color.red);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
    }
}