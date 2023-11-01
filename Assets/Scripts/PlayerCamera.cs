using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] WallRun wallRun;
    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;

    [SerializeField] private float xSen;
    [SerializeField] private float ySen;
    private float xRotation;
    private float yRotation;
    private float mouseX;
    private float mouseY;
    private float multiplier = 0.01f;

    void Start()
    {
        DisableCursor();
    }

    void Update()
    {
        MInput();

        cam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void MInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * xSen * multiplier;
        xRotation -= mouseY * ySen * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    private static void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
