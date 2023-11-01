using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Animations;
[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    [SerializeField] private float camTilt = 25f;
    [SerializeField] private float camTiltTime = 25f;
     private float tilt;
    [Header("Movement")]
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float runSpeed = 8f;
    private CharacterController controller;
   
    private Vector3 playerVelocity = Vector3.zero;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private bool useGravity;
    [SerializeField] private float gravityCounterForce = 10f;
    private bool canMove = true;

    
    [SerializeField] private float lookSpeed = 4f;
    private float lookXlimit = 90f;
    private float rotation = 0;

    [Header("Sliding")]
    private bool isSliding;
    [SerializeField] private float slideTime = 1f;
    private float maxSlideTime = 1f;
    [SerializeField] private float slideForce;

    //For Slope
    private bool slopeSlide = true;
    private float slopeSpeed = 12f;
    private Vector3 slopePoint;
    private bool isSlopeSliding;

    [Header("Wall Run")]
    private bool isWallRunning;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallRunTime = 2f;
    private float wallRunMaxTime = 2f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallSideJump;

    [Header("Exit Wall Jump")]
    private bool isExitWall;
    [SerializeField] private float exitWallTime = .2f;
    private float exitWallMaxTime = .2f;

    //DetectWall
    private float wallDistance = .5f;
    private float minJumpHeight = 1f;
    [SerializeField] private LayerMask Wall;
    [SerializeField] private LayerMask Ground;
    private bool wallLeft;
    private bool wallRight;
    RaycastHit rightWallHit, leftWallHit;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        DisableCursor();
    }

    void Update()
    {        
        Movement();
        CheckWallRun();
    }

    private void Movement()
    {

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //Run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : playerSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : playerSpeed) * Input.GetAxis("Horizontal") : 0;
        float moveDirectionY = playerVelocity.y;
        playerVelocity = (forward * curSpeedX) + (right * curSpeedY);

        if (slopeSlide && Slope() && isSlopeSliding)
        {
            playerVelocity += new Vector3(slopePoint.x, -slopePoint.y, slopePoint.z) * slopeSpeed;
        }

        //Crouch
        CrouchAndSlide(isRunning);

        //Jump
        Jump(moveDirectionY);

        //Rotation
        Rotate();
    }
    
    //Wall Run
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isForward = Input.GetKey(KeyCode.W);
        bool isRun = Input.GetKey(KeyCode.LeftShift);

        if (hit.gameObject.tag == ("Wall") && isForward && AboveGround() && !isRun)
        {
            StartWallRun();
            if (isWallRunning)
            {
                WallRunMovement();
            }
               

            if(Input.GetButton("Jump"))
            {
                StartWallJump();
                if (isExitWall)
                    WallJump();
            }
        }
        else
        {
            wallRunTime = wallRunMaxTime;
            exitWallTime = exitWallMaxTime;
        }
    }

    private void CheckWallRun()
    {
        wallRight = Physics.Raycast(transform.position, Vector3.right, out rightWallHit, wallDistance, Wall);
        wallLeft = Physics.Raycast(transform.position, Vector3.left, out leftWallHit, wallDistance, Wall);

        if((wallLeft || wallRight) && !isWallRunning)
        {
            WallRunMovement();
        }
        if((!wallLeft || !wallRight) && isWallRunning)
        {
            StopWallRun();
        }
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, Ground);
    }
    private void StartWallRun()
    {
        isWallRunning = true;
        playerVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);

        if(wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        }
        else if(wallRight)
        {
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);
        }
    }
    private void WallRunMovement()
    {
        useGravity = true;

        Vector3 wallNormal = wallLeft ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forward = Vector3.Cross(wallNormal, transform.up);

        playerVelocity = (forward * wallRunForce * Input.GetAxisRaw("Vertical"));

        if (useGravity)
        {
            playerVelocity = transform.up * gravityCounterForce;
        }
        else
        {
            playerVelocity = transform.up * gravityValue;
        }
        wallRunTime -= Time.deltaTime;

        if (wallRunTime <= 0)
        {
            StopWallRun();
        }
    }
    private void StopWallRun()
    {
        isWallRunning = false;
        isExitWall = true;
        useGravity = false;
        tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
    }
    
    //Wall Jump
    private void StartWallJump()
    {
        isExitWall = true;
    }
    private void WallJump()
    {
        Vector3 wallNormal = wallLeft ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForceApply = transform.up * wallJumpForce + wallNormal * wallSideJump;  

        playerVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
        playerVelocity = wallForceApply;
    
        exitWallTime -= Time.deltaTime;

        if(exitWallTime < 0)
        {
            StopWallJump();
        }
    }
    private void StopWallJump()
    {
        isExitWall = true;
    }

    private void Jump(float moveDirectionY)
    {
        if (Input.GetButton("Jump") && canMove && controller.isGrounded && Slope() == false && !isWallRunning)
        {
            playerVelocity.y = jumpHeight;
        }
        else
        {
            playerVelocity.y = moveDirectionY;
        }
        if (!controller.isGrounded)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
        }
    }
    
    private void CrouchAndSlide(bool isRunning)
    {
        bool isCrouching = Input.GetKey(KeyCode.C);
        bool isForward = Input.GetKey(KeyCode.W);
        bool isBackward = Input.GetKey(KeyCode.S);

        if (isCrouching && !isRunning)
        {
            transform.localScale = new Vector3(1, 0.6f, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (isCrouching && isRunning && (isForward || isBackward))
        {
            StartSliding();
            if (isSliding)
                SlidingMovement();
        }
       
        else
        {
            Reset();
        }
    }

    private void Reset()
    {
        slideTime = maxSlideTime;
    }
    private void StartSliding()
    {
        isSliding = true;
        transform.localScale = new Vector3(1, 0.2f, 1);
    }

    private void SlidingMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
    
        playerVelocity = (forward.normalized * slideForce * Input.GetAxisRaw("Vertical"));
        slideTime -= Time.deltaTime;

        if (slideTime < 0)
        {
            StopSliding();
        }
    }

    private void StopSliding()
    {
        isSliding = false;
        transform.localScale = new Vector3(1, 1, 1);
    }

    private bool Slope()
    {
        isSlopeSliding = true;
        if(controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 3f))
        {
            slopePoint = slopeHit.normal;
            return Vector3.Angle(slopePoint, Vector3.up ) > controller.slopeLimit;
        }

        else
        {
            return false;
        }
    }
    private void Rotate()
    {
        controller.Move(playerVelocity * Time.deltaTime);
        if (canMove)
        {
            rotation += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation = Mathf.Clamp(rotation, -lookXlimit, lookXlimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private static void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
