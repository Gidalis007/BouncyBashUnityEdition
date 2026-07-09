using System;
using UnityEngine;

public class WallColliderController : MonoBehaviour
{
    /*private CarController _myCarController;

    private void Awake()
    {
        _myCarController = gameObject.GetComponentInParent<CarController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "ColliderDetector" || other.tag == "Untagged" || other.tag == "Brush")
        {
            
            StopCoroutine("StopSpeed");
            StartCoroutine(_myCarController.StopSpeed(0.7f));
            
            _myCarController.MoveForce = - _myCarController.MoveForce*0.5f;
            //_myCarController.MoveForce = Vector3.ClampMagnitude(_myCarController.MoveForce, _myCarController.MaxSpeed*2);


        }
    }*/
    
    [Header("Réglages du rebond")]
    [Tooltip("Vitesse minimale garantie après un rebond")]
    [SerializeField] private float _minBounceSpeed = 5f;
    [SerializeField] private float _maxBounceSpeed = 30f;

    [Tooltip("Pourcentage d'énergie conservé après le rebond (1 = rebond parfait, <1 = perte d'énergie)")]
    [Range(0f, 3f)]
    [SerializeField] private float _bounciness = 1f;

    [Tooltip("Tag utilisé pour identifier les murs")]
    [SerializeField] private string[] _wallTags;

    [Header("Anti-collage au mur")]
    [Tooltip("Distance dont l'objet est repoussé hors du mur à chaque frame de contact (0 = désactivé)")]
    public float _pushForce = 2f;
    
    private Rigidbody _rigidbody;
    

    
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Bounce(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsWall(collision.gameObject)) return;
        Vector3 normal = collision.contacts[0].normal;
        _rigidbody.AddForce(normal * _pushForce, ForceMode.Force);
    }

    private void Bounce(Collision collision)
    {
        if (!IsWall(collision.gameObject)) return;
        
        Vector3 normal = collision.contacts[0].normal;
        Vector3 incomingVelocity = _rigidbody.linearVelocity;
        Vector3 reflected = Vector3.Reflect(incomingVelocity, normal) * _bounciness;
        

        // minimum speed
        if (reflected.magnitude < _minBounceSpeed)
        {
            Vector3 direction = reflected.magnitude > 0.01f ? reflected.normalized : normal;
            reflected = direction * _minBounceSpeed;
        }
        
        // maximum speed
        reflected = Vector3.ClampMagnitude(reflected, _maxBounceSpeed);

        _rigidbody.linearVelocity = reflected;
    }
    
    private bool IsWall(GameObject obj)
    {
        for (int i = 0; i < _wallTags.Length; i++)
        {
            if (obj.CompareTag(_wallTags[i])) return true;
        }
        return false;
    }
}
