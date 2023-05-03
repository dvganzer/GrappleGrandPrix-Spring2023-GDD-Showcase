using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Walking")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    public bool readyToJump = true;

    [Header("Controls")]
    public Vector2 movementInput = Vector2.zero;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    float horitzontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public Grappling grappleState;
    public Swinging swingState;

    public AnimatorHandler animatorHandler;

    public enum MovementState
    {
        running,
        grappling,
        swinging,
        air
    }

    public PlayerInput playerInput;
    public enum InputDevice {  controller = 0, keyboard = 1}

    private void StateHandler()
    {
        if (grounded)
        {
            state = MovementState.running;
        }
        else if (swingState.isSwinging)
        {
            state = MovementState.swinging;
            animatorHandler.anim.Play("Swing");
            airMultiplier = 3f;
        }
        else if (grappleState.isGrappling)
        {
            state = MovementState.grappling;
            animatorHandler.anim.Play("Pull");
        }
        else if (!grappleState.isGrappling && !swingState.isSwinging)
        {
            airMultiplier = 2f;
            animatorHandler.anim.Play("In Air");
        }
        else
            state = MovementState.air;
            
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        animatorHandler.Initialize();
    }

    void Update()
    {
        
       // Debug.Log(rb.velocity);
        animatorHandler.UpdateAnimatorValues(movementInput.y, movementInput.x);
        

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        SpeedControl();

        
        //Drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
            

    }

    private void FixedUpdate()
    {
        StateHandler();
        //PlayerMovement
        moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

    }


    public void Jump(InputAction.CallbackContext context)
    {
        //Jump
        if (context.performed && readyToJump && grounded)
        {
            readyToJump = false;
            animatorHandler.anim.Play("Jump");
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    

}
