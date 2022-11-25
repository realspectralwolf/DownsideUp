using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    Rigidbody _rb;
    public static event System.Action<Vector3> Touched;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_rb.velocity.magnitude > 1)
        {
            Touched?.Invoke(transform.position);
        }
    }
}
