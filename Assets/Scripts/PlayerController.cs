using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public static PlayerController instance;
    Vector2 input;
    Rigidbody _rb;
    bool isGrounded = false;
    List<Rigidbody> collidersInRange = new List<Rigidbody>();
    bool isHolding = false;
    Rigidbody draggableRb;

    CapsuleCollider _collider;
    float startMoveSpeed;
    bool canMove = true;
    bool isRoomRotated = false;
    float _footstepsStopDelay = 0.2F;
    float _footstepsTimer = 0;
    Vector3 holdOffset;

    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] float distToGround;
    [SerializeField] LayerMask jumpableLayer;
    [SerializeField] Animator _animator;
    [SerializeField] LineRenderer lineRend;
    [SerializeField] Transform lineStartPoint;
    [SerializeField] AudioSource footstepsAudiosource;
    [SerializeField] AudioClip audioLaserOn;
    [SerializeField] AudioClip audioJump;
    [SerializeField] float rotSmoothing;
    [SerializeField] PhysicMaterial frictionMat;
    [SerializeField] PhysicMaterial slipperyMat;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        ExitScript.PlayerExited += OnExitLevel;
        RestartMgr.gameStarted += GameStarted;
        RestartMgr.roomRotateStart += OnRoomRotate;
    }

    private void OnDisable()
    {
        ExitScript.PlayerExited -= OnExitLevel;
        RestartMgr.gameStarted -= GameStarted;
        RestartMgr.roomRotateStart -= OnRoomRotate;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        lineRend.positionCount = 0;
        startMoveSpeed = moveSpeed;
        _rb.centerOfMass = Vector3.zero;
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (!isRoomRotated) return;

        if (isGrounded && Input.GetButton("Jump") && _rb.velocity.y <= 0)
        {
            _rb.AddForce(Vector3.up * jumpForce);
            AudioSource.PlayClipAtPoint(audioJump, _rb.transform.position);
            isGrounded = false;
        }

        HandleObjectGrabbingSystem();
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();
        HandlePlayerRotation();

        if (canMove)
        {
            HandlePlayerMovement();
            FootstepSystemFixedUpdate();
            ApplyAdditionalGravity();
        }

        UpdateAnimatorVariables();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, distToGround);
    }

    #endregion

    #region Methods

    private void GameStarted()
    {
        transform.SetParent(null);
        
        groundCheckPoint.localPosition = -groundCheckPoint.localPosition;
        canMove = true;
        isRoomRotated = true;
    }

    private void OnRoomRotate()
    {
        canMove = false;
    }

    private void OnExitLevel()
    {
        gameObject.SetActive(false);
    }

    private void HandleObjectGrabbingSystem()
    {

        if (Input.GetButtonDown("Fire1") && !isHolding)
        {
            StartHolding();
        }

        if (!Input.GetButton("Fire1") && isHolding)
        {
            EndHolding();
        }

        if (Input.GetButtonUp("Fire1") && isHolding)
        {
            EndHolding();
        }

        if (isHolding)
        {
            lineRend.positionCount = 2;
            DrawLine();
        }
        else
        {
            lineRend.positionCount = 0;
        }

        _animator.SetBool("isHolding", isHolding);
    }

    private void StartHolding()
    {
        if (collidersInRange.Count > 0)
        {
            draggableRb = collidersInRange[0];

            // get closest
            for (int i = 0; i < collidersInRange.Count; i++)
            {
                if (Vector3.Distance(_rb.transform.position, collidersInRange[i].transform.position) < Vector3.Distance(_rb.transform.position, draggableRb.transform.position))
                {
                    draggableRb = collidersInRange[i];
                } 
            }

            lineRend.positionCount = 2;
            AudioSource.PlayClipAtPoint(audioLaserOn, _rb.transform.position);
            draggableRb.gameObject.GetComponent<Outline>().enabled = true;

            holdOffset = draggableRb.transform.position - _rb.transform.position;

            draggableRb.freezeRotation = true;
            isHolding = true;
        }
    }

    private void EndHolding()
    {
        isHolding = false;
        lineRend.positionCount = 0;
        draggableRb.gameObject.GetComponent<Outline>().enabled = false;
            
        if (draggableRb.velocity.y > 0)
        {
            Vector3 newVel = draggableRb.velocity;
            newVel.y = 0;
            draggableRb.velocity = newVel;
        }

        draggableRb.freezeRotation = false;
    }

    public void EnterGrabRange(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            collidersInRange.Add(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    public void ExitGrabRange(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            collidersInRange.Remove(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    private void HandlePlayerRotation()
    {
        if (canMove && GetMoveDirection().magnitude != 0)
        {
            Vector3 newRot = _rb.rotation.eulerAngles;
            newRot.y = Quaternion.LookRotation(_rb.transform.position + GetMoveDirection() * moveSpeed * 3, Vector3.up).eulerAngles.y;
            Quaternion qRot = Quaternion.Euler(newRot);

            _rb.MoveRotation(Quaternion.Slerp(_rb.transform.rotation, qRot, rotSmoothing));
        }
    }

    private void HandlePlayerMovement()
    {
        if (isHolding)
        {
            Vector3 velocityChange = _rb.velocity - draggableRb.velocity;
            velocityChange.y /= 4;
            draggableRb.AddForce(velocityChange, ForceMode.VelocityChange);
            moveSpeed = startMoveSpeed / 1.5F;
        }
        else
        {
            moveSpeed = startMoveSpeed;
        }

        if (isGrounded)
        {
            _collider.material = frictionMat;
        }
        else
        {
            _collider.material = slipperyMat;
        }

        _rb.AddForce(GetMoveDirection() * moveSpeed);
    }

    private Vector3 GetMoveDirection() // diagonal movement mapping
    {
        Vector3 dir = Vector3.forward * input.y + Vector3.right * input.y;
        dir += -Vector3.forward * input.x + Vector3.right * input.x;
        return dir;
    }

    private float GetWalkVelocityMagnitude()
    {
        Vector3 mVelocity = _rb.velocity;
        mVelocity.y = 0;
        return mVelocity.magnitude;
    }

    private void UpdateAnimatorVariables()
    {
        if (canMove)
        {
            
            _animator.SetFloat("xSpeed", GetWalkVelocityMagnitude());
        }
        else
        {
            _animator.SetFloat("xSpeed", 0);
        }
        _animator.SetBool("isGrounded", isGrounded);
    }

    private void ApplyAdditionalGravity()
    {
        if (!isGrounded)
            _rb.AddForce(-Vector3.up * 20, ForceMode.Acceleration);
    }

    private void FootstepSystemFixedUpdate()
    {
        _footstepsTimer += Time.fixedDeltaTime;

        if (isGrounded && _rb.velocity.y > -1 && _rb.velocity.y < 1 && GetWalkVelocityMagnitude() > 1)
        {
            footstepsAudiosource.enabled = true;
            _footstepsTimer = 0;
        }
        else if (_footstepsTimer >= _footstepsStopDelay)
        {
            footstepsAudiosource.enabled = false;
        }
    }

    private bool IsGrounded()  
    {
        return Physics.CheckSphere(groundCheckPoint.position, distToGround, jumpableLayer);
    }

    private void DrawLine()
    {
        lineRend.SetPosition(0, lineStartPoint.position);
        Vector3 secondPos = draggableRb.transform.position + draggableRb.centerOfMass;
        lineRend.SetPosition(1, secondPos);
    }

    #endregion
}
