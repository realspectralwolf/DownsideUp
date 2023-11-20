using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb.velocity.magnitude > AudioManager.Instance.MinImpactVelocity)
        {
            AudioManager.Instance.PlayImpact(transform.position);
        }
    }
}
