using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class Swinging : MonoBehaviour
{
    [Header("Swinging")]
    [SerializedField] public  LineRenderer lr;
    [SerializedField] private Vector3 grapplePoint;
    [SerializedField] public LayerMask whatIsGrappleable;

    [SerializedField] public Transform gunTip;
    [SerializedField] public Transform cameraSpot;
    [SerializedField] public Transform player;
    [SerializedField] public bool grappling = false;

    [SerializeField] private float maxDistance;
    [SerializedField] private SpringJoint joint;
    [SerializedField] public AudioSource hookHit;

    [Header("OdmGear")]
    [SerializedField] public Transform orientation;
    [SerializedField] private Rigidbody rb;
    [SerializedField] public float horizontalThrustForce;
    [SerializedField] public float forwardThrustForce;
    [SerializedField] public float extendCableSpeed;

    [Header("Prediction")]
    [SerializedField] public RaycastHit predictionHit;
    [SerializedField] public float predictionSphereCastRadius;
    [SerializedField] public Transform predictionPoint;
    [SerializedField] public bool isSwinging;


    public Vector2 movementInput = Vector2.zero;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckForSwingPoints();
        if(joint != null)
        {
            if (movementInput == new Vector2(-1,0) )
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
    }

    public
        void OdmGearMovement(InputAction.CallbackContext context)
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

            if (isSwinging)
            {
                hookHit.Play();
            }
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
            DestroyJoints();
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

        if(realHitPoint!= Vector3.zero)
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

    void DestroyJoints()
    {
        // Get all joints attached to the current GameObject
        Joint[] joints = GetComponents<Joint>();

        // Loop through all joints and destroy them
        foreach (Joint joint in joints)
        {
            Destroy(joint);
        }
    }
}
