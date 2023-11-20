using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabbing : MonoBehaviour
{
    [SerializeField] PlayerController controller;

    private void OnTriggerEnter(Collider other)
    {
        controller.AddColiderToGrabRange(other);
    }

    private void OnTriggerExit(Collider other)
    {
        controller.RemoveColliderFromGrabRange(other);
    }
}