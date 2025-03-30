using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Camera playerCamera;

    public float lookXLimit = 45f;
    public float lookSpeed = 2f;

    float inputX;
    float inputY;
    private void Update()
    {
        inputX += Input.GetAxis("Mouse X") * lookSpeed;
        inputY += Input.GetAxis("Mouse Y") * lookSpeed;
        inputY = Mathf.Clamp(inputY, -lookXLimit, lookXLimit);



        Quaternion rotation = Quaternion.Euler(-inputY, inputX, 0);
        transform.rotation = rotation;


    }
}
