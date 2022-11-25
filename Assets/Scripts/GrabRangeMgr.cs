using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRangeMgr : MonoBehaviour
{
    [SerializeField] PlayerController controller;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered");
        controller.EnterGrabRange(other);
    }

    private void OnTriggerExit(Collider other)
    {
        controller.ExitGrabRange(other);
    }
}