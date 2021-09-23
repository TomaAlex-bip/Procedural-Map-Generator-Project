using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookScript : MonoBehaviour
{

    [Range(0f, 5f)]
    [SerializeField] private float mouseSensitivity;

    private float lerpSpeed = 500f;

    private Transform cam;

    float xRot = 0f;


    private void Start()
    {
        cam = transform.Find("Camera");

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {

        SimpleRotation();




        //LerpRotation();

    }

    private void SimpleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }


    // needs more tweaking
    private void LerpRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 50f * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        Quaternion rotationX = Quaternion.Euler(xRot, 0f, 0f);
        Quaternion rotationY = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseX, transform.rotation.eulerAngles.z);

        cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, rotationX, lerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationY, lerpSpeed * Time.deltaTime);
    }


}
