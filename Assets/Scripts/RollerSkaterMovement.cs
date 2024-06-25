using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;
using static UnityEngine.InputSystem.InputAction;

public class RollerSkaterMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigidBody;
    [SerializeField]
    private float movementForce;
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float maximumSkateUpSpeed;
    [SerializeField]
    private float maximumSkateSpeed;
    [SerializeField]
    private float midAirDirectionalInfluence;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float playerTurnSpeed;
    [SerializeField]
    private CinemachineVirtualCameraBase mainCam;
    [SerializeField]
    private GameObject modelHandler;
    [SerializeField]
    private GameObject grappleObjectPrefab;
    private GameObject grappleObjectInWorld;
    [SerializeField]
    private GameObject grappleLineRendererPrefab;
    private GameObject grappleLineRendererInWorld;
    [SerializeField]
    private float groundedTolerance;
 
    private LineRenderer grappleLineRenderer;

    private Vector3 movementDirection;
    private bool weMovin;
    private bool jump;
    [SerializeField]
    private bool isGrounded;
    private ConfigurableJoint playerJoint;
    private bool grappleToggle;

    public void Start()
    {
        playerJoint = gameObject.GetComponent<ConfigurableJoint>();
        grappleToggle = false;
        grappleLineRendererInWorld = Instantiate(grappleLineRendererPrefab);
        grappleLineRenderer = grappleLineRendererInWorld.GetComponent<LineRenderer>();
        grappleLineRenderer.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMove(CallbackContext input)
    {
        Vector2 inputDirection = input.ReadValue<Vector2>();
        Vector3 inputDirVec3 = new Vector3(inputDirection.x, 0.0f, inputDirection.y);
        movementDirection = inputDirVec3;
        
        if (input.phase != InputActionPhase.Canceled)
        {
            weMovin = true;
        }
        else
        {
            weMovin = false;
        }

    }

    public void OnJump(CallbackContext input)
    {
        jump = input.ReadValueAsButton();
        InputActionPhase iap = input.phase;
        if (iap == InputActionPhase.Performed && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce);
        }
    }

    public void OnGrapple(CallbackContext input)
    {
        InputActionPhase iap = input.phase;
        if(iap == InputActionPhase.Performed)
        {
            Vector3 lookingDirection = mainCam.transform.forward;
            Vector3 cameraPosition = mainCam.transform.position;
            RaycastHit hit;
            if (!grappleToggle)
            {
                Collider[] buildingsSurrounding = Physics.OverlapSphere(rigidBody.position, 40.0f);
                Vector3 directionOfMomentum = rigidBody.velocity.normalized;
                Vector3 testDirection = lookingDirection;
                Collider grappleCandidate = null;
                float smallestAngle = 90.0f;
                foreach (Collider building in buildingsSurrounding)
                {
                    if(building.gameObject.tag.Equals("hookable"))
                    {
                        Vector3 playerToBuilding = (building.transform.position - rigidBody.position).normalized;
                        float currentAngle = Vector3.Angle(testDirection, playerToBuilding);
                        if (currentAngle < smallestAngle)
                        {
                            smallestAngle = currentAngle;
                            grappleCandidate = building;
                        }
                    }
                }
                if (grappleCandidate != null)
                {
                    grappleToggle = true;
                    Vector3 min = grappleCandidate.bounds.min;
                    Vector3 max = grappleCandidate.bounds.max;
                    Vector3[] boundTopPoints = new Vector3[4];
                    boundTopPoints[0] = max;
                    boundTopPoints[1] = new Vector3(max.x, max.y, min.z);
                    boundTopPoints[2] = new Vector3(min.x, max.y, min.z);
                    boundTopPoints[3] = new Vector3(min.x, max.y, max.z);
                    float minAngle = 180.0f;
                    Vector3 candidatePosition = max;
                    for(int i = 0; i < 4; i++)
                    {
                        Vector3 fromPlayerToCandidate = boundTopPoints[i] - rigidBody.position;
                        fromPlayerToCandidate.y = 0;
                        fromPlayerToCandidate.Normalize();
                        float currentAngle = Vector3.Angle(testDirection, fromPlayerToCandidate);
                        if(currentAngle < minAngle)
                        {
                            candidatePosition = boundTopPoints[i];
                            minAngle = currentAngle;
                        }
                    }
                    //Vector3 closestPointToBuilding = grappleCandidate.ClosestPointOnBounds(rigidBody.position + Vector3.up * 40.0f);
                    grappleObjectInWorld = Instantiate(grappleObjectPrefab, candidatePosition, Quaternion.identity);
                    playerJoint.connectedBody = grappleObjectInWorld.transform.GetChild(0).GetComponent<Rigidbody>();
                    GrappleOn(playerJoint);
                }
            }
            else
            {
                grappleToggle = false;
                GrappleOff(playerJoint);
                Destroy(grappleObjectInWorld);
            }
        }
    }

    void Update()
    {
        CheckGrounded();
        Vector3 facingDirection = rigidBody.velocity;
        facingDirection.y = 0.0f;
        facingDirection.Normalize();
        modelHandler.transform.forward = Vector3.Lerp(modelHandler.transform.forward, facingDirection, playerTurnSpeed * Time.deltaTime);

        float mvForce = isGrounded ? movementForce : midAirDirectionalInfluence;
        if (weMovin)
        {
            //get the direction the camera is facing
            Vector3 camFacingDirection = gameObject.transform.position - mainCam.transform.position;
            camFacingDirection.y = 0.0f;
            camFacingDirection.Normalize();

            //find out the angle from which the camera is rotated away from "forward"
            Quaternion forward = Quaternion.LookRotation(Vector3.forward);
            Quaternion cameraLocalRotation = Quaternion.FromToRotation(Vector3.forward, camFacingDirection);
            float angleOfRotation = Quaternion.Angle(forward, cameraLocalRotation);
                
            //rotate the inputted direction by the angle between the camera facing vector and forward
            Vector3 finalMovementDirection = Quaternion.AngleAxis(angleOfRotation, Vector3.Dot(Vector3.right, camFacingDirection) > 0 ? Vector3.up : Vector3.down) * movementDirection;
            rigidBody.AddForce(finalMovementDirection * mvForce * Time.deltaTime);
        }

        RenderGrapple();
    }

    private void RenderGrapple()
    {
        if (grappleToggle)
        {
            Vector3[] positions = new Vector3[] 
            { 
                rigidBody.transform.position, 
                grappleObjectInWorld.transform.position 
            };
            grappleLineRenderer.SetPositions(positions);
        }
    }

    private void CheckGrounded()
    {
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, groundedTolerance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void GrappleOn(ConfigurableJoint jnt)
    {
        jnt.xMotion = ConfigurableJointMotion.Limited;
        jnt.yMotion = ConfigurableJointMotion.Limited;
        jnt.zMotion = ConfigurableJointMotion.Limited;
        jnt.angularXMotion = ConfigurableJointMotion.Free;
        jnt.angularYMotion = ConfigurableJointMotion.Free;
        jnt.angularZMotion = ConfigurableJointMotion.Free;
        grappleLineRenderer.enabled = true;
    }

    private void GrappleOff(ConfigurableJoint jnt)
    {
        jnt.xMotion = ConfigurableJointMotion.Free;
        jnt.yMotion = ConfigurableJointMotion.Free;
        jnt.zMotion = ConfigurableJointMotion.Free;
        jnt.angularXMotion = ConfigurableJointMotion.Free;
        jnt.angularYMotion = ConfigurableJointMotion.Free;
        jnt.angularZMotion = ConfigurableJointMotion.Free;
        grappleLineRenderer.enabled = false;
    }
}
