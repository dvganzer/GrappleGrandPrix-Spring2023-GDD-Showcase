using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombinedCode : MonoBehaviour
{
   

    void Update()
    {
        
        #region UpdateJump&Movement
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
        #endregion

        #region UpdateSwinging
        CheckForSwingPoints();
        if (joint != null)
        {
            if (movementInput == new Vector2(-1, 0))
            {
                rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
            }
            if (movementInput == new Vector2(1, 0))
            {
                rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
            }
            if (movementInput == new Vector2(0, 1))
            {
                rb.AddForce(orientation.right * forwardThrustForce * Time.deltaTime);
            }
            if (movementInput == new Vector2(0, -1))
            {
                float extendedDistanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;
                joint.maxDistance = extendedDistanceFromPoint * 0.8f;
                joint.minDistance = extendedDistanceFromPoint * 0.25f;
            }
        }
        #endregion

        #region UpdateGrappling
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
        #endregion

    }
    #region 1stPersonMovement
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
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
   
    private void FixedUpdate()
    {
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

    #endregion

    #region Jump
    public void Jump(InputAction.CallbackContext context)
    {
        //Jump
        if (context.performed && readyToJump && grounded)
        {
            readyToJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    #endregion

    #region Swinging
    [Header("Swinging")]
    [SerializedField] public LineRenderer lr;
    [SerializedField] private Vector3 grapplePoint;
    [SerializedField] public LayerMask whatIsGrappleable;

    [SerializedField] public Transform gunTip;
    [SerializedField] public Transform cameraSpot;
    [SerializedField] public Transform player;
    [SerializedField] public bool grappling = false;

    [SerializeField] private float maxDistance;
    [SerializedField] private SpringJoint joint;

    [Header("OdmGear")]
    [SerializedField] public float horizontalThrustForce;
    [SerializedField] public float forwardThrustForce;
    [SerializedField] public float extendCableSpeed;

    [Header("Prediction")]
    [SerializedField] public RaycastHit predictionHit;
    [SerializedField] public float predictionSphereCastRadius;
    [SerializedField] public Transform predictionPoint;

    [SerializedField] public bool isSwinging;

    public void OdmGearMovement(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void Cable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            
            Vector3 distanceToPoint = grapplePoint - transform.position;
            rb.AddForce(distanceToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    public void StartSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSwinging = true;
            
            if (predictionHit.point == Vector3.zero) return;


            grapplePoint = predictionHit.point;
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
        if (context.canceled)
        {
            isSwinging = false;
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

    private void CheckForSwingPoints()
    {
        if (joint != null)
            return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cameraSpot.position, predictionSphereCastRadius, cameraSpot.forward, out sphereCastHit, maxDistance, whatIsGrappleable);
        RaycastHit raycastHit;
        Physics.Raycast(cameraSpot.position, cameraSpot.forward, out raycastHit, maxDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        else
        {
            realHitPoint = Vector3.zero;
        }

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    #endregion

    #region Grappling
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
    public bool isGrappling;
    public void StartGrapple(InputAction.CallbackContext context)
    {
        if (grapplingCdTimer > 0) return;

        isGrappling = true;

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
        grapplePoints = gunTip.position;
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

        isGrappling = false;

        grapplingCdTimer = grapplingCd;

        lrs.enabled = false;
    }

    public bool IsGrapplings()
    {
        return isGrappling;
    }

    public Vector3 GetGrapplePoints()
    {
        return grapplePoints;
    }
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {


        velocityToSet = CalculateJumpVelocity(player.transform.position, targetPosition, trajectoryHeight);
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
