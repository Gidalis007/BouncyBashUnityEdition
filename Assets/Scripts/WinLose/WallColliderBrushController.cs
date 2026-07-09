using System;
using UnityEngine;

public class WallColliderBrushController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = gameObject.GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "ColliderDetector" && other.tag != "Player")
        {
            Vector3 bumpDirection = new Vector3(-(_rigidbody.linearVelocity.x*1.5f), 
                                                0, 
                                                -(_rigidbody.linearVelocity.z)*1.5f);
            
            _rigidbody.linearVelocity = bumpDirection;
            //_myCarController.MoveForce = Vector3.ClampMagnitude(_myCarController.MoveForce, _myCarController.MaxSpeed*2);


        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "ColliderDetector" && other.tag != "Player")
        {
            _rigidbody.AddForce(other.transform.forward * 10f, ForceMode.Impulse);
        }
    }
}
