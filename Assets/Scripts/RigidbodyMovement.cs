using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class RigidbodyMovement : MonoBehaviour
{
    private Rigidbody rb;
    Vector3 moveDirection;
    Vector3 slopeDirection;

    [Header("Movement")]
    private float playerHeight = 2f;
    public float playerSpeed = 6f;
    public float movementMultiplier = 10f;
    [SerializeField] float airMultiplier = 0.4f;
    private float verticalInput;
    private float horizontalInput;
    [Header("Ground Dectection")]
    [SerializeField] LayerMask groundMask;
    bool isGrounded;
    private float groundDistance = 0.4f;

    [Header("Drag")]
    private float groundDrag = 6f;
    private float airDrag = 2f;

    [Header("Run")]
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float acceleration = 10f;

    [Header("Crouch")]
    [SerializeField] float crouchSpeed = 20f;

    [Header("Sliding")]
    private bool isSliding;
    [SerializeField] private float slideTime = 1f;
    private float maxSlideTime = 1f;
    [SerializeField] private float slideForce;

    [Header("Jump")]
    public float jumpForce = 5f;


    [Header("KeyBind")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.C;

    RaycastHit slopeHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

   
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, 1, 0), groundDistance, groundMask);

        MoveInput();
        ControlDrag();
        ControlRun();
        Crouch();
        Jump();
        Sliding();
        slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }
    void FixedUpdate()
    {
        MoveDirection();
    }
    private void MoveInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput);
    }

    private void MoveDirection()
    {
        if(isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * playerSpeed * movementMultiplier, ForceMode.Acceleration);
            rb.drag = groundDrag;
        }
        else if(isGrounded && OnSlope())
        {
            rb.AddForce(slopeDirection.normalized * playerSpeed * movementMultiplier, ForceMode.Acceleration);
           
        }
        else if(!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * playerSpeed * airMultiplier, ForceMode.Acceleration);
            
        }
    }
    
    private void Crouch()
    {
        if (Input.GetKey(crouchKey) && isGrounded)
        {
            transform.localScale = new Vector3(1, 0.7f, 1);
            playerSpeed = Mathf.Lerp(playerSpeed, crouchSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            playerSpeed = Mathf.Lerp(playerSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }
    private void Sliding()
    {
        if(Input.GetKey(crouchKey) && Input.GetKey(runKey) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
        {
            StartSliding();
            if(isSliding)
            {
                SlidingMovement();
            }
        }
        else
        {
            ResetSlide();
        }
    }

    private void ResetSlide()
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

        moveDirection = (forward.normalized * slideForce);
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

    private void Jump()
    {
        if(Input.GetKey(jumpKey) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    private void ControlRun()
    {
        if(Input.GetKey(runKey) && isGrounded)
        {
            playerSpeed = Mathf.Lerp(playerSpeed, runSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            playerSpeed = Mathf.Lerp(playerSpeed, walkSpeed, acceleration * Time.deltaTime);
        }    
    }
    private void ControlDrag()
    {
        if(isGrounded)
        {
            rb.drag = groundDrag;
        }
        else if(!isGrounded)
        {
            rb.drag = airDrag;
        }
    }
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight/2 + 0.5f ))
        {
            if(slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
