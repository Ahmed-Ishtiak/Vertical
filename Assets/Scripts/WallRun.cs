using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    private Rigidbody rb;
    private RigidbodyMovement rbMovement;
    [SerializeField] Transform orientation;

    [Header("Wall Run")]
    float wallRunDistance = 0.6f;
    [SerializeField] float minJumpHeight = 1.5f;
    [SerializeField] float wallRunForce = 10f;
    [SerializeField] float wallRunTime = 2f;
    [SerializeField] float maxWallRunTime = 2f;

    [Header("Wall Jump")]
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    [Header("Exit Wall")]
    private bool exitWall;
    private float exitWallTime;
    public float maxExitWallTime;

    [Header("Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask wallMask;
    RaycastHit leftWallHit, rightWallHit;
    bool wallLeft, wallRight;

    [Header("Input")]
    float verticalInput;
    float horizontalInput;
    KeyCode wallJump = KeyCode.Space;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("Fov & Tilt")]
    public Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunFov;
    [SerializeField] private float wallRunFovTime;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltTime;

    public float tilt { get; private set; }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbMovement = GetComponent<RigidbodyMovement>();
    }
    void Update()
    {
        CheckWall();
        StateMachine();
    }
    void FixedUpdate()
    {
        if (rbMovement.isWallRun)
            WallRunMovement();
    }
    private void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right,out leftWallHit ,wallRunDistance,wallMask);
        wallRight = Physics.Raycast(transform.position, orientation.right,out rightWallHit ,wallRunDistance,wallMask);
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight,groundMask);
    }

    private void StateMachine()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
      
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitWall)
        {
            if (!rbMovement.isWallRun)
            {
                StartWallRun();
            }
            if(wallRunTime >= 0)
            {
                wallRunTime -= Time.deltaTime;
            }
            if(wallRunTime <= 0 && rbMovement.isWallRun)
            {
                exitWall = true;
                exitWallTime = maxExitWallTime;
            }
            if (Input.GetKeyDown(wallJump))
            {
                WallJump();
            }     
        }
        else if (exitWall)
        {
            if (rbMovement.isWallRun)
                StopWallRun();
            if (exitWallTime >= 0)
                exitWallTime -= Time.deltaTime;
            if (exitWallTime <= 0)
                exitWall = false;
        }
        else
        {
            if(rbMovement.isWallRun)
            {
                StopWallRun();
            }
        }
    }
    private void StartWallRun()
    {
        rbMovement.isWallRun = true;
        wallRunTime = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunFov, wallRunFovTime * Time.deltaTime);

    }
    private void WallRunMovement()
    {
        rb.useGravity = useGravity;
       
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forward = Vector3.Cross(wallNormal, transform.up);


        if (wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);
        }

        if ((orientation.forward - forward).magnitude > (orientation.forward - -forward).magnitude)
        {
            forward = -forward;
        }
        
        rb.AddForce(forward * wallRunForce, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }
    private void StopWallRun()
    {
        rb.useGravity = true;
        rbMovement.isWallRun = false;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunFovTime * Time.deltaTime);
        tilt = Mathf.Lerp(0f, 0f, camTiltTime * Time.deltaTime);
    }

    private void WallJump()
    {
        exitWall = true;
        exitWallTime = maxExitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceApply, ForceMode.Impulse);
    }
}
