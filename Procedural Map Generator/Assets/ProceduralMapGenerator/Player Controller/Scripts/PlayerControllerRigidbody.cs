using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerRigidbody : MonoBehaviour
{

    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpPower;

    [SerializeField] private float maxVelocity;


    private Rigidbody rb;

    private float moveX;
    private float moveZ;

    private Vector3 velocity;


    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }


    private void Update()
    {
        moveX = Input.GetAxis("Vertical");
        moveZ = Input.GetAxis("Horizontal");

        velocity = new Vector3(moveX, 0.0f, moveZ) * movementSpeed * Time.deltaTime;

        velocity.x = Mathf.Clamp(velocity.x, -maxVelocity, maxVelocity);
        velocity.z = Mathf.Clamp(velocity.z, -maxVelocity, maxVelocity);


    }

    private void FixedUpdate()
    {

        //rb.velocity = (transform.forward * moveX + transform.right * moveZ) * movementSpeed * Time.fixedDeltaTime + transform.up;

        //rb.AddRelativeForce(new Vector3(moveX, 0.0f, moveZ) * movementSpeed * Time.fixedDeltaTime);

        //rb.MovePosition(transform.position + new Vector3(moveX, 0.0f, moveZ) * movementSpeed * Time.fixedDeltaTime);

        rb.AddRelativeForce(velocity, ForceMode.VelocityChange);
        

    }

}
