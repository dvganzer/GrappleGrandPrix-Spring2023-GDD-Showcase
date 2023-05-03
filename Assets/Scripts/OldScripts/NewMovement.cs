using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NewMovement : MonoBehaviour
{
    [Header("Camera")]
    [SerializedField] public Vector2 cameraInput = Vector2.zero;

    [SerializedField] private Vector2 currentInputVector;
    [SerializedField] private Vector2 smoothInputVelocity;
    


    [SerializedField] public float senseX =.5f;
    [SerializedField] public float senseY = .5f;

    [SerializedField] public Transform orientation;

    [SerializedField] public float xRotation;
    [SerializedField] public float yRotation;

    [Header("Movement")]
    [SerializedField] public Vector2 moveInput = Vector2.zero;

    [SerializedField] public float moveSpeed;
    [SerializedField] Vector3 moveDirection;
     Rigidbody rb;

    [Header("Ground Check")]
    [SerializedField] public float groundDrag;
    [SerializedField] public float airDrag;
    [SerializedField] public float playerHeight;
    [SerializedField] public LayerMask whatisGround;
    [SerializedField] public bool grounded;

    [Header("Jumping")]
    [SerializedField] public float jumpForce;
    [SerializedField] public float jumpCooldown;
    [SerializedField] public float airMultiplier;
    [SerializedField] public bool readyToJump = true;

    [Header("UI")]
    [SerializedField] public Text SenseText;



    [Header("Swinging")]
    [SerializedField] private LineRenderer lr;
    [SerializedField] private Vector3 grapplePoint;
    [SerializedField] public LayerMask whatIsGrappleable;

    [SerializedField] public Transform gunTip;
    [SerializedField] public Transform cameraSpot;
    [SerializedField] public Transform player;
    [SerializedField] public bool grappling = false;

    [SerializeField] private float maxDistance;
    [SerializedField] private SpringJoint joint;

    [HideInInspector]
    public AnimatorHandler animatorHandler;
    public NewMovement newMovement;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        animatorHandler.Initialize();
    }

    void Update()
    {
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        animatorHandler.UpdateAnimatorValues(moveInput.y, moveInput.x);

        SpeedControl();
        //Camera
       
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        float cameraX = cameraInput.x * senseX;
        float cameraY = cameraInput.y * senseY;

        yRotation += cameraX;
        xRotation -= cameraY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Grounded
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + .2f, whatisGround);
        if (grounded)
            rb.drag = groundDrag;
        else 
            rb.drag = airDrag;
       
    }
    private void FixedUpdate()
    {
        //Movement
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
            rb.mass = 1;
        }


        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.mass = 20;
            if(!grounded && !grappling && !grapplings)
            {
                animatorHandler.anim.Play("In Air");
            }
            
            if (grappling)
            {
                animatorHandler.anim.Play("Swing");
            }

            if (grapplings)
            {
                animatorHandler.anim.Play("Pull");
            }
           
        }




    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        cameraInput = context.ReadValue<Vector2>() ;
        
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && readyToJump && grounded)
        {

            readyToJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce,  ForceMode.Impulse);
            rb.AddForce(transform.forward * jumpForce, ForceMode.Impulse);
            animatorHandler.anim.Play("Jump");
            Invoke(nameof(ResetJump), jumpCooldown);

        }
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    public void SenseUp(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            senseX += .1f;
            senseY += .1f;
            SenseText.text = "SENSITIVITY:" + senseX *10;
        }

    }
    public void SenseDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            senseX -= .1f;
            senseY -= .1f;
            SenseText.text = "SENSITIVITY:" + senseX *10;
        }
            
    }

    #region Swinging 

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        newMovement = GetComponent<NewMovement>();

    }
    void LateUpdate()
    {
        DrawRope();
        if (grapplings)
            lrs.SetPosition(0, gunTip.position);
    }


    public void OnSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraSpot.position, cameraSpot.forward, out hit, maxDistance, whatIsGrappleable))
            {

                grapplePoint = hit.point;
                grappling = true;
                joint = player.gameObject.AddComponent<SpringJoint>();

                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);


                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;


                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 4.5f;

                lr.positionCount = 2;
                currentGrapplePosition = gunTip.position;
            }
        }
        if (context.canceled)
        {
            lr.positionCount = 0;
            Destroy(joint);
            grappling = false;
        }

    }
    private Vector3 currentGrapplePosition;

    void DrawRope()
    {

        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
    #endregion

    #region Pull
    [Header("References")]
    public bool freeze = false;
    public Transform cam;
    
    public LayerMask whatIsPullable;
    public LineRenderer lrs;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    private Vector3 grapplePoints;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    

    private bool grapplings;

    public void StartGrapple(InputAction.CallbackContext context)
    {
        if (grapplingCdTimer > 0) return;

        grapplings = true;

        freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsPullable))
        {
            grapplePoints = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoints = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lrs.enabled = true;
        lrs.SetPosition(1, grapplePoints);
    }

    private void ExecuteGrapple()
    {
        freeze = false;
        
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoints.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        JumpToPosition(grapplePoints, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        freeze = false;

        grapplings = false;

        grapplingCdTimer = grapplingCd;

        lrs.enabled = false;
    }

    public bool IsGrapplings()
    {
        return grapplings;
    }

    public Vector3 GetGrapplePoints()
    {
        return grapplePoints;
    }
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {


        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);


    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {

        rb.velocity = velocityToSet;

        // cam.DoFov(grappleFov);
    }
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    #endregion
}

internal class SerializedFieldAttribute : Attribute
{
}