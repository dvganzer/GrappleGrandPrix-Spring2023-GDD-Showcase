using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
   
    [Header("References")]
    public bool freeze = false;
    public Transform cam;
    public Transform gunTip;
    public AudioSource zip;

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

    Rigidbody rb;

    [Header("Prediction")]
    [SerializedField] public RaycastHit predictionHit;
    [SerializedField] public float predictionSphereCastRadius;
    [SerializedField] public Transform predictionPoint;

   

   

    public bool isGrappling;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

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
        lrs.SetPosition(0, gunTip.position);
        lrs.SetPosition(1, grapplePoints);
    }

    private void ExecuteGrapple()
    {
        freeze = false;
        //grapplePoints = gunTip.position;
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoints.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        JumpToPosition(grapplePoints, highestPointOnArc);
        zip.Play();

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

    private void CheckForSwingPoints()
    {
       /* if (joint != null)
            return; */

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxGrappleDistance, whatIsPullable);
        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxGrappleDistance, whatIsPullable);

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

}    
