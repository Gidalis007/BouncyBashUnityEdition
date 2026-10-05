using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WallBounceController : NetworkBehaviour
{
    #region Serialized Fields
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
    [SerializeField] private float _pushForce = 2f;
    #endregion

    #region Attribute
    private Rigidbody _rigidbody;
    private float _lastWallSoundTime = -10f;
    private const float WallSoundCooldown = 0.15f;
    #endregion Attribute

    #region Mono
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

    #endregion Mono

    #region Methods
    private void Bounce(Collision collision)
    {
        if (!IsWall(collision.gameObject)) return;

        Vector3 normal = collision.contacts[0].normal;
        Vector3 incomingVelocity = _rigidbody.linearVelocity;
        Vector3 reflected = Vector3.Reflect(incomingVelocity, normal) * _bounciness;


        if (reflected.magnitude < _minBounceSpeed)
        {
            Vector3 direction = reflected.magnitude > 0.01f ? reflected.normalized : normal;
            reflected = direction * _minBounceSpeed;
        }

        reflected = Vector3.ClampMagnitude(reflected, _maxBounceSpeed);

        _rigidbody.linearVelocity = reflected;

        if (IsServer && Time.time - _lastWallSoundTime >= WallSoundCooldown)
        {
            _lastWallSoundTime = Time.time;
            PlayWallBounceRpc(transform.position, Mathf.Clamp01(incomingVelocity.magnitude / 20f));
        }
    }

    private bool IsWall(GameObject obj)
    {
        for (int i = 0; i < _wallTags.Length; i++)
        {
            if (obj.CompareTag(_wallTags[i])) return true;
        }
        return false;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayWallBounceRpc(Vector3 position, float intensity01)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAt("wall_bounce", position, 0.6f + 0.4f * Mathf.Clamp01(intensity01));
        }
    }

    #endregion

}
