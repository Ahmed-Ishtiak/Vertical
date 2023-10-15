using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private Vector3 playerVelocity = Vector3.zero;
    private bool groundedPlayer; 
    private float gravityValue = -9.81f;
    private bool canMove = true;

    [SerializeField] private float lookSpeed = 4f;
    private float lookXlimit = 90f;
    private float rotation = 0;


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

        //for running
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : playerSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : playerSpeed) * Input.GetAxis("Horizontal") : 0;
        float moveDirectionY = playerVelocity.y;
        playerVelocity = (forward * curSpeedX) + (right * curSpeedY);

        //Crouch
        bool isCrouching = Input.GetKey(KeyCode.C);
        if (isCrouching && isRunning == false) 
        {
            transform.localScale = new Vector3(1, 0.6f, 1);    
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        //for jump
        if(Input.GetButton("Jump") && canMove && controller.isGrounded)
        {
            playerVelocity.y = jumpHeight; 
        }
        else
        {
            playerVelocity.y = moveDirectionY;
        }
        if(!controller.isGrounded)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
        }

        //for rotation
        controller.Move(playerVelocity * Time.deltaTime);
        if(canMove)
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
