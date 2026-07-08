using System;
using UnityEngine;

public class ForwardColliderController : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private float _bumpForce = 1.2f;
    [SerializeField] private float _jumpBumpForce = 5f;

    #endregion SerializeFields
    
    #region Attributes
    
    private Collider _forwardCollider;
    private CarController _myCarController;
    
    #endregion  Attributes

    private void Awake()
    {
        _forwardCollider = GetComponent<Collider>();
        _myCarController = gameObject.GetComponentInParent<CarController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 bumpDirection;
        CarController AdverseCar = other.gameObject.GetComponentInParent<CarController>();
        
        if ( !other.gameObject.GetComponentInParent<CarController>() || AdverseCar.CanBeBump != true )
            return;
        
        AdverseCar.CanBeBump = false;
        
        if (other.tag == "Player")
        {
            bumpDirection.x =  _myCarController.Rigidbody.linearVelocity.x 
                               * _bumpForce;
            bumpDirection.z =  _myCarController.Rigidbody.linearVelocity.z 
                               * _bumpForce;
            bumpDirection.y = _myCarController.Rigidbody.linearVelocity.y
                              +_jumpBumpForce
                              *_bumpForce;
            
            AdverseCar.Rigidbody.AddForce(bumpDirection, ForceMode.Impulse);
        }
    }
}
