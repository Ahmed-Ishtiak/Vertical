using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    private Rigidbody rb;
    private RigidbodyMovement rbMovement;
    [SerializeField] Transform orientation;


    [Header("Vault")]
    private bool isVaulting;
    public float vaultSpeed;
    [SerializeField] float vaultTime = 0.5f;
    [SerializeField] float vaultMaxTime = 0.5f;

    [Header("Detection")]
    [SerializeField] LayerMask wallMask;
    public float sphereCastRadius;
    public float sphereLength;
    private float vaultAngle = 30;
    private float vaultMaxAngle = 30;


    RaycastHit wallHit;
    bool wallFront;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbMovement = GetComponent<RigidbodyMovement>();
    }

   
    void Update()
    {
        WallCheck();
        StateMachine();

        if(isVaulting)
        {
            VaultMovement();
        }
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward ,out wallHit, sphereLength ,wallMask);
        vaultAngle = Vector3.Angle(orientation.forward, -wallHit.normal);

        if(rbMovement.isGrounded)
        {
            vaultTime = vaultMaxTime;
        }
    }

    private void StateMachine()
    {
        if (wallFront && (vaultAngle < vaultMaxAngle) && Input.GetKey(KeyCode.W)) 
        {
            if(!isVaulting && vaultTime > 0)
            {
                StartVault();
            }
            if(vaultTime > 0)
            {
                vaultTime -= Time.deltaTime;
            }
            if(vaultTime <= 0)
            {
                StopVault();
            }
        }
        else
        {
            if(isVaulting)
            {
                StopVault();
            }  
        }
    }

    private void StartVault()
    {
        isVaulting = true;
        rbMovement.isVaulting = true;
    }
    private void VaultMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, vaultSpeed, rb.velocity.z);
    }
    private void StopVault()
    {
        isVaulting = false;
        rbMovement.isVaulting = false;
    }
    
}
