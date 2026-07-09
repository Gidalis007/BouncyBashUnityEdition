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
        CarController adverseCar = other.gameObject.GetComponentInParent<CarController>();
        PainterController painterController = other.gameObject.GetComponent<PainterController>();
        
        if ( (!other.gameObject.GetComponentInParent<CarController>() || adverseCar.CanBeBump != true ) && (!other.gameObject.GetComponent<PainterController>()))
            return;
        

        
        if (other.tag == "Player")
        {
            adverseCar.CanBeBump = false;
            
            bumpDirection.x =  _myCarController.Rigidbody.linearVelocity.x 
                               * _bumpForce;
            bumpDirection.z =  _myCarController.Rigidbody.linearVelocity.z 
                               * _bumpForce;
            bumpDirection.y = _myCarController.Rigidbody.linearVelocity.y
                              +_jumpBumpForce
                              *_bumpForce;
            adverseCar?.Rigidbody.AddForce(bumpDirection, ForceMode.Impulse);
        }
        
        else if (other.tag == "Brush")
        {
            bumpDirection.x =  _myCarController.Rigidbody.linearVelocity.x 
                               * _bumpForce;
            bumpDirection.z =  _myCarController.Rigidbody.linearVelocity.z 
                               * _bumpForce;
            bumpDirection.y = _myCarController.Rigidbody.linearVelocity.y
                              //+_jumpBumpForce
                              *_bumpForce;
            painterController?.Rigidbody.AddForce(bumpDirection, ForceMode.Impulse);
        }
    }
}
