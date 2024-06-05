using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;
    [SerializeField]
    private float cameraRegularSpeed = 6f;
    [SerializeField]
    private float cameraFastSpeed = 12f;
    private float angle;
    private float rotationOffset;

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        RotateCamera();
    }

    private void MoveCamera()
    {
        float horizontalMovement = Input.GetAxis("Horizontal") * cameraSpeed;
        float verticalMovement = Input.GetAxis("Vertical") * cameraSpeed;
        bool moving = false;

        if (Input.GetKey(KeyCode.W))
        {
            angle = 45f;
            moving = true;
            if (Input.GetKey(KeyCode.A))
                angle = 90f;
            if (Input.GetKey(KeyCode.D))
                angle = 0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            angle = 225f;
            moving = true;
            if (Input.GetKey(KeyCode.A))
                angle = 180f;
            if (Input.GetKey(KeyCode.D))
                angle = 270f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            angle = 315f;
            moving = true;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            angle = 135f;
            moving = true;
        }

        if (!moving)
            return;

        cameraSpeed = Input.GetKey(KeyCode.LeftShift) ? cameraFastSpeed : cameraRegularSpeed;

        angle += rotationOffset;
        Vector3 movement = Quaternion.Euler(0, 0, angle) * Vector3.up;
        transform.position += movement * (Time.deltaTime * cameraSpeed);
    }

    private void RotateCamera()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            rotationOffset -= 90f;
        if (Input.GetKeyDown(KeyCode.E))
            rotationOffset += 90f;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotationOffset);
    }
}
