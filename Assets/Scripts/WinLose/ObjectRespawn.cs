using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectRespawn : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _ogLocation;
    private Quaternion _ogRotation;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _ogLocation = _rb.position;
        _ogRotation = _rb.rotation;
    }

    public void Respawn()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.position = _ogLocation;
        _rb.rotation = _ogRotation;
    }
}
