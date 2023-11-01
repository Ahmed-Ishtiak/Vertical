using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 playerDirection;
    [SerializeField] Transform orientation;

    [Header("Wall Run")]
    private bool isWallRun;
    [SerializeField] private float wallRunForce = 5f;
    [SerializeField] private float wallRunTime = 2f;
    private float maxWallRunTime = 2f;

    [Header("Detect Wall Run")]
    [SerializeField] LayerMask wallMask;
    [SerializeField] LayerMask groundMask;
    private float wallRunDistance = 0.5f;
    private float minJumpHeight = 1.5f;
    private bool wallLeft, wallRight;
    private RaycastHit leftWallHit, rightWallHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        CheckWall();
        AboveGround();

        if(Input.GetKey(KeyCode.Space) && Input.GetAxisRaw("Vertical") > 0 && AboveGround())
        {
            StartWallRun();
            if(isWallRun)
            {
                WallRunMove();
            }
        }
        else
        {
            ResetWallRun();
        }
    }

    private void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, Vector3.left, out leftWallHit, wallRunDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, Vector3.right, out rightWallHit, wallRunDistance, wallMask);
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }    
    private void StartWallRun()
    {
        isWallRun = true;
        playerDirection = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * wallRunForce, ForceMode.Force);
    }
    private void WallRunMove()
    {
        Vector3 wallNormal = wallLeft ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forward = Vector3.Cross(wallNormal, transform.up);

        playerDirection = (forward * wallRunForce * Input.GetAxisRaw("Vertical"));

        wallRunTime -= Time.deltaTime;
        if(wallRunTime < 0)
        {
            StopWallRun();
        }
    }
    private void StopWallRun()
    {
        isWallRun = false;
    }

    private void ResetWallRun()
    {
        wallRunTime = maxWallRunTime;
    }
}
