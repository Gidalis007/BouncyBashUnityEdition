using System;
using UnityEngine;

public class WallColliderController : MonoBehaviour
{
    private CarController _myCarController;

    private void Awake()
    {
        _myCarController = gameObject.GetComponentInParent<CarController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "ColliderDetector" || other.tag == "Untagged")
        {
            
            StopCoroutine("StopSpeed");
            StartCoroutine(_myCarController.StopSpeed(0.7f));
            
            _myCarController.MoveForce = - _myCarController.MoveForce*0.5f;
            _myCarController.MoveForce = Vector3.ClampMagnitude(_myCarController.MoveForce, _myCarController.MaxSpeed*2);


        }
    }
}
