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
    public Camera playerCamera;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float runSpeed = 8f;
    private CharacterController controller;
    //For moving
    private Vector3 playerVelocity = Vector3.zero;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private bool canMove = true;

    //For Rotation
    [SerializeField] private float lookSpeed = 4f;
    private float lookXlimit = 90f;
    private float rotation = 0;

    //For Sliding
    private bool isSliding;
    [SerializeField] private float slideTime = 1f;
    private float maxSlideTime = 1f;
    [SerializeField] private float slideForce;

    //For Slope
    private bool slopeSlide = true;
    private float slopeSpeed = 12f;
    private Vector3 slopePoint;
    private bool isSlopeSliding;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        DisableCursor();
    }

    void Update()
    {
        Movement();
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

    private void Jump(float moveDirectionY)
    {
        if (Input.GetButton("Jump") && canMove && controller.isGrounded && Slope() == false)
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
        if (isCrouching && isRunning && (isForward || isBackward) && !OnSlope())
        {
            StartSliding();
            if (isSliding)
                SlidingMovement();
        }
        else if (isCrouching && isRunning && (isForward || isBackward) && OnSlope())
        {
            StartSliding();
            if (isSliding)
            {
                OnSlopeSliding();
            }
        }

        else
        {
            Reset();
        }
    }

    private void OnSlopeSliding()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        playerVelocity = (forward.normalized * slideForce * Input.GetAxisRaw("Vertical"));

        if (slideTime < 0 || -playerVelocity.y < 0)
        {
            StopSliding();
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
        Vector3 right = transform.TransformDirection(Vector3.right);
    
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

    private bool OnSlope()
    {
        isSlopeSliding = true;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, controller.height * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < controller.slopeLimit && angle != 0;
        }
        else
        {
            return false;
        }
    }

    private static void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
