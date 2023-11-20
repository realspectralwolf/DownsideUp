using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region Variables

    Rigidbody rb;
    bool isGrounded = false;
    List<Rigidbody> collidersInRange = new List<Rigidbody>();
    Rigidbody grabbedRb, closestRb = null;
    Vector2 inputMove = Vector2.zero;
    bool inputJump = false;

    LineRenderer lineToGrabbedRb;
    CapsuleCollider capsuleCollider;

    [SerializeField] PlayerData data;
    [SerializeField] Animator animator;
    [SerializeField] Transform hand;
    [SerializeField] Transform head;
    [SerializeField] Transform groundCheck;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;

        capsuleCollider = GetComponent<CapsuleCollider>();
        lineToGrabbedRb = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        HandleClosestObjectOutline();

        if (grabbedRb)
        {
            RenderLineToGrabbedRb();
        }
    }

    private void FixedUpdate()
    {
        HandleGroundCheck();
        HandleFriction();
        HandleRotation();
        HandleMovement();
        HandleJump();
        HandleHeldItemMovement();
        HandleFootsteps();
        HandleAnimatorParams();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, data.GroundCheckRadius);

        Gizmos.DrawLine(transform.position, transform.position + GetMoveDirection());
    }

    #endregion

    #region Methods

    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, data.GroundCheckRadius, data.GroundLayer);
    }

    private void HandleFriction()
    {
        float frictionValue = isGrounded ? 1f : 0f;

        capsuleCollider.material.dynamicFriction = frictionValue;
        capsuleCollider.material.staticFriction = frictionValue;
    }

    private void HandleClosestObjectOutline()
    {
        if (grabbedRb != null) return;

        closestRb = GetClosestRb();

        for (int i = 0; i < collidersInRange.Count; i++)
        {
            if (collidersInRange[i] == closestRb)
            {
                Outline outline = closestRb.gameObject.GetComponent<Outline>();
                outline.OutlineColor = data.ClosestItemColor;
                outline.enabled = true;
            }
            else
            {
                collidersInRange[i].gameObject.GetComponent<Outline>().enabled = false;
            }
        }
    }

    private void HandleRotation()
    {
        if (GetMoveDirection().magnitude != 0)
        {
            Vector3 newRot = rb.rotation.eulerAngles;
            newRot.y = Quaternion.LookRotation(GetMoveDirection(), Vector3.up).eulerAngles.y;
            rb.MoveRotation(Quaternion.Slerp(rb.transform.rotation, Quaternion.Euler(newRot), data.RotSmoothness));
        }
    }

    private void HandleMovement()
    {
        if (rb.isKinematic) return;

        ApplyAdditionalGravity();

        float currentWalkVelocity = data.WalkVelocity;
        if (grabbedRb)
        {
            Vector3 playerForce = Vector3.zero;
            playerForce.y = grabbedRb.transform.position.y - rb.transform.position.y;
            rb.AddForce(playerForce * 1, ForceMode.Acceleration);
            currentWalkVelocity = data.HoldingWalkVelocity;
        }

        rb.AddForce(GetMoveDirection() * currentWalkVelocity);
    }

    private void HandleHeldItemMovement()
    {
        if (!grabbedRb) return;

        Vector3 otherRbForce = rb.transform.position - grabbedRb.transform.position;
        otherRbForce.y -= data.HeldItemAdditionalGravity;

        float multiplier = Vector3.Distance(rb.transform.position, grabbedRb.transform.position);
        otherRbForce.x *= multiplier * data.SpringStiffness.x;
        otherRbForce.y *= multiplier * data.SpringStiffness.y;
        otherRbForce.z *= multiplier * data.SpringStiffness.z;

        grabbedRb.AddForce(otherRbForce, ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        if (!inputJump) return;
        inputJump = false;

        if (!isGrounded) return;
        rb.AddForce(Vector3.up * data.JumpForce);
        AudioManager.Instance.PlaySound(Sound.jump);
        isGrounded = false;
    }

    private void HandleFootsteps()
    {
        if (isGrounded && rb.velocity.y > -1 && rb.velocity.y < 1 && GetWalkVelocityMagnitude() > 1)
        {
            AudioManager.Instance.PlayFootsteps = true;
        }
        else
        {
            AudioManager.Instance.PlayFootsteps = false;
        }
    }

    private float GetDistanceTo(Transform target)
    {
        return Vector3.Distance(rb.transform.position + rb.transform.forward * data.TargetForwardBias, target.position);
    }

    private Rigidbody GetClosestRb()
    {
        Rigidbody rbInRange = (collidersInRange.Count > 0) ? collidersInRange[0] : null;
        for (int i = 0; i < collidersInRange.Count; i++)
        {
            if (GetDistanceTo(collidersInRange[i].transform) < GetDistanceTo(rbInRange.transform))
            {
                rbInRange = collidersInRange[i];
            }
        }
        return rbInRange;
    }    

    private void GrabClosestRb()
    {
        if (closestRb != null)
        {
            grabbedRb = closestRb;
            grabbedRb.freezeRotation = true;

            Outline outline = grabbedRb.gameObject.GetComponent<Outline>();
            outline.OutlineColor = data.HeldItemColor;
            outline.enabled = true;

            grabbedRb.gameObject.layer = 0;

            lineToGrabbedRb.enabled = true;
            AudioManager.Instance.PlaySound(Sound.itemGrabbed);
        }
    }

    private void DropGrabbedRb()
    {
        if (grabbedRb != null)
        {
            grabbedRb.freezeRotation = false;
            grabbedRb.gameObject.GetComponent<Outline>().enabled = false;

            if (grabbedRb.velocity.y > 0)
            {
                Vector3 newVel = grabbedRb.velocity;
                newVel.y = 0;
                grabbedRb.velocity = newVel;
            }

            grabbedRb.gameObject.layer = data.DraggableLayerID;

            grabbedRb = null;
            lineToGrabbedRb.enabled = false;

            AudioManager.Instance.PlaySound(Sound.itemDropped);
        }
    }

    public void AddColiderToGrabRange(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            collidersInRange.Add(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    public void RemoveColliderFromGrabRange(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            collidersInRange.Remove(other.gameObject.GetComponent<Rigidbody>());

            if (other.gameObject.GetComponent<Rigidbody>() != grabbedRb)
            {
                other.gameObject.GetComponent<Outline>().enabled = false;
            }
        }
    }

    private Vector3 GetMoveDirection() // diagonal movement mapping
    {
        Vector3 dir = Vector3.forward * inputMove.y + Vector3.right * inputMove.x;
        dir.Normalize();

        Quaternion rotation = Quaternion.Euler(0, 45, 0);     
        Vector3 rotatedDir = rotation * dir;

        return rotatedDir;
    }

    private float GetWalkVelocityMagnitude()
    {
        Vector3 mVelocity = rb.velocity;
        mVelocity.y = 0;
        return mVelocity.magnitude;
    }

    private void HandleAnimatorParams()
    {
        animator.SetFloat("xSpeed", GetWalkVelocityMagnitude());
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isHolding", grabbedRb);
    }

    private void ApplyAdditionalGravity()
    {
        if (!isGrounded)
            rb.AddForce(-Vector3.up * 20, ForceMode.Acceleration);
    }

    private void RenderLineToGrabbedRb()
    {
        lineToGrabbedRb.positionCount = 2;
        lineToGrabbedRb.SetPosition(0, hand.position);
        Vector3 secondPos = grabbedRb.transform.position + grabbedRb.centerOfMass;
        lineToGrabbedRb.SetPosition(1, secondPos);
    }

    public void MoveGroundCheckToHead()
    {
        groundCheck.position = head.position;
    }

    #endregion

    #region Inputs
    public void OnMove(InputValue value)
    {
        inputMove = value.Get<Vector2>();
    }

    public void OnGrab(InputValue value)
    {
        float actionState = value.Get<float>();
        if (actionState == 1)
        {
            GrabClosestRb();
        }
        else if(actionState == 0)
        {
            DropGrabbedRb();
        }
    }

    public void OnJump()
    {
        inputJump = true;
    }

    public void OnRestart()
    {
        LevelsManager.Instance.RestartLevel();
    }

    public void OnBack()
    {
        GameplayManager.Instance.Pause();
    }

    #endregion
}
