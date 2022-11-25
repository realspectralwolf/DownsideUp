using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] float distToGround;
    [SerializeField] LayerMask jumpableLayer;
    [SerializeField] Animator _animator;
    Vector2 input;
    Rigidbody _rb;
    Vector3 lastPos;
    bool isGrounded = false;
    List<Rigidbody> collidersInRange = new List<Rigidbody>();
    bool isHolding = false;
    Rigidbody draggableRb;
    [SerializeField] LineRenderer lineRend;
    [SerializeField] Transform lineStartPoint;
    [SerializeField] AudioSource footstepsAudiosource;
    [SerializeField] AudioClip audioLaserOn;
    [SerializeField] AudioClip audioJump;
    [SerializeField] float rotSmoothing;
    [SerializeField] PhysicMaterial frictionMat;
    [SerializeField] PhysicMaterial slipperyMat;
    CapsuleCollider _collider;
    float startMoveSpeed;
    bool canMove = true;
    bool isRoomRotated = false;
    float _footstepsStopDelay = 0.2F;
    float _footstepsTimer = 0;
    Vector3 holdOffset;
    Vector3 holdClosestVertex;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        ExitScript.PlayerExited += OnExitLevel;
        RestartMgr.gameStarted += GameStarted;
        RestartMgr.roomRotateStart += OnRoomRotate;
    }

    void OnDisable()
    {
        ExitScript.PlayerExited -= OnExitLevel;
        RestartMgr.gameStarted -= GameStarted;
        RestartMgr.roomRotateStart -= OnRoomRotate;
    }

    void GameStarted()
    {
        transform.SetParent(null);
        
        groundCheckPoint.localPosition = -groundCheckPoint.localPosition;
        canMove = true;
        isRoomRotated = true;
    }

    void OnRoomRotate()
    {
        canMove = false;
    }

    void OnExitLevel()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        lineRend.positionCount = 0;
        lastPos = _rb.transform.position;
        startMoveSpeed = moveSpeed;
        _rb.centerOfMass = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (!isRoomRotated) return;

        if (isGrounded && Input.GetButton("Jump") && _rb.velocity.y <= 0 )
        {
            _rb.AddForce(Vector3.up * jumpForce);
            AudioSource.PlayClipAtPoint(audioJump, _rb.transform.position);
            isGrounded = false;
        }

        #region GrabbingSystem

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

        #endregion
    }

    void StartHolding()
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

            // TESTING
            holdOffset = draggableRb.transform.position - _rb.transform.position;
            //draggableRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            //draggableRb.interpolation = RigidbodyInterpolation.None;

            draggableRb.freezeRotation = true;

            //Find closest vertex (disable, not working as expected)
            /*
            Vector3 myPos = lineStartPoint.position;
            Vector3 draggablePos = draggableRb.transform.position;

            Vector3 localClosestVertex = new Vector3(200, 200, 200);
            Vector3[] faces = draggableRb.gameObject.GetComponent<MeshFilter>().mesh.normals;
        
            for(int i = 0; i < faces.Length; i++)
            {
                float currentDistance = Vector3.Distance(myPos, draggablePos + localClosestVertex);
                float newDistance = Vector3.Distance(myPos, draggablePos + faces[i]);

                if (newDistance < currentDistance)
                {
                    localClosestVertex = faces[i];
                }
            }

            holdClosestVertex = localClosestVertex;
            */

            isHolding = true;
        }
    }

    void EndHolding()
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

        //draggableRb.interpolation = RigidbodyInterpolation.Interpolate;
        //AudioSource.PlayClipAtPoint(audioLaserOff, _rb.transform.position);
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

    void FixedUpdate()
    {
        isGrounded = IsGrounded();

        Vector3 moveVector = Vector3.zero;
        moveVector += Vector3.forward * input.y * moveSpeed + Vector3.right * input.y * moveSpeed;
        moveVector += -Vector3.forward * input.x * moveSpeed + Vector3.right * input.x * moveSpeed;

        if (canMove)
        {
            Vector3 mVelocity = _rb.velocity;
            mVelocity.y = 0;
            _animator.SetFloat("xSpeed", mVelocity.magnitude);
            //_animator.SetFloat("xSpeed", moveVector.magnitude);
        }
        else
        {
            _animator.SetFloat("xSpeed", 0);
            footstepsAudiosource.enabled = false;
        }

        #region PlayerRotation

        if (canMove && moveVector.magnitude != 0)
        {
            Vector3 newRot = _rb.rotation.eulerAngles;
            newRot.y = Quaternion.LookRotation(_rb.transform.position + 3*moveVector, Vector3.up).eulerAngles.y;
            Quaternion qRot = Quaternion.Euler(newRot);

            _rb.MoveRotation(Quaternion.Slerp(_rb.transform.rotation, qRot, rotSmoothing)); 
        }  

        #endregion

        if (!canMove) return;

        #region FootstepsAudioSystem

        _footstepsTimer += Time.fixedDeltaTime;

        if (isGrounded && _rb.velocity.y > -1 && _rb.velocity.y < 1 && moveVector != Vector3.zero)
        {
            footstepsAudiosource.enabled = true;
            _footstepsTimer = 0;
        }
        else if (_footstepsTimer >= _footstepsStopDelay)
        {
            footstepsAudiosource.enabled = false;
        }

        #endregion

        _rb.AddForce(moveVector);

        if (isGrounded)
        {
            _collider.material = frictionMat;
        }
        else
        {
            _collider.material = slipperyMat;
        }

        _animator.SetBool("isGrounded", isGrounded);

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

        // add more gravity
        if (!isGrounded) 
            _rb.AddForce(-Vector3.up * 20, ForceMode.Acceleration);

        lastPos = _rb.transform.position;
    }

    bool IsGrounded()  
    {
        //return Physics.Raycast(groundCheckPoint.position, -Vector3.up, distToGround);
        return Physics.CheckSphere(groundCheckPoint.position, distToGround, jumpableLayer);
        //return Physics.CheckSphere(groundCheckPoint.position, distToGround);
    }

    void DrawLine()
    {
        lineRend.SetPosition(0, lineStartPoint.position);
        Vector3 secondPos = draggableRb.transform.position + draggableRb.centerOfMass;
        lineRend.SetPosition(1, secondPos);

        //lineRend.SetPosition(1, draggableRb.transform.position + holdClosestVertex);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, distToGround);
    }
}
