using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float sprintMultiplier = 1f;

    //[Range(1f, 5f)]
    [SerializeField] private float gravityMultiplier = 1f;

    [SerializeField] private bool isGrounded;
    [SerializeField] private LayerMask groundCheckLayer;


    const float GRAVITY = -9.81f;
    float gravity;

    CharacterController controller;
    Transform groundCheck;

    float xMovement;
    float zMovement;
    bool jump;
    bool sprint;
    bool crouch;

    Vector3 velocity;

    float originalStepOffset;
    float updatedMovementSpeed;
    float originalHeight;


    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        groundCheck = transform.Find("GroundCheck");

        // just apply the multiplier to the gravity constant
        UpdateGravity();

        originalStepOffset = controller.stepOffset;
        originalHeight = transform.localScale.y;
    }


    private void Update()
    {
        GetInput();

        UpdateGravity();

        CheckGrounding();

        UpdateMovement();

        UpdateJumping();



    }


    private void GetInput()
    {
        xMovement = Input.GetAxis("Horizontal");
        zMovement = Input.GetAxis("Vertical");
        jump = Input.GetKeyDown(KeyCode.Space);
        sprint = Input.GetKey(KeyCode.LeftShift);
        crouch = Input.GetKey(KeyCode.C);
    }

    // WASD movement
    private void UpdateMovement()
    {
        if(crouch)
        {
            updatedMovementSpeed = movementSpeed / 2f;
            transform.localScale = new Vector3(transform.localScale.x, originalHeight / 2f, transform.localScale.z);
        }
        else if(sprint && isGrounded)
        {
            updatedMovementSpeed = movementSpeed * sprintMultiplier;
        }
        else if(sprint && jump)
        {
            updatedMovementSpeed = movementSpeed * sprintMultiplier;
        }
        else if(!sprint)
        {
            updatedMovementSpeed = movementSpeed;
        }

        if(!crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, originalHeight, transform.localScale.z);
        }

        Vector3 move = transform.forward * zMovement + transform.right * xMovement;
        controller.Move(move * updatedMovementSpeed * Time.deltaTime);
    }

    // Check if Jumping and simulate gravity for the character
    private void UpdateJumping()
    {
        if (jump && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            controller.stepOffset = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        // nu stiu inca de ce, dar daca mut inmultirea cu Time.deltaTime sus face urat
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateGravity() => gravity = GRAVITY * gravityMultiplier;

    // check if the character is on ground
    private void CheckGrounding()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.35f, groundCheckLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
            controller.stepOffset = originalStepOffset;
            updatedMovementSpeed = movementSpeed;
        }
    }




}
