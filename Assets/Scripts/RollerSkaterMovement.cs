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
    private float maxSpeedClamp;
    [SerializeField]
    private float movementSpeed;
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
    [SerializeField]
    private GameObject grappleObjectInWorld;

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
            grappleToggle = !grappleToggle;
            if (grappleToggle)
            {
                GrappleOff(playerJoint);
                Destroy(grappleObjectInWorld);
            }
            else
            {
                grappleObjectInWorld = Instantiate(grappleObjectPrefab, new Vector3(-9.192033f, 7.69f, -8.753658f), Quaternion.identity);
                playerJoint.connectedBody = grappleObjectInWorld.transform.GetChild(0).GetComponent<Rigidbody>();
                GrappleOn(playerJoint);
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

        if (isGrounded)
        {
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
                rigidBody.AddForce(finalMovementDirection * movementForce * Time.deltaTime);
            }
        }
    }

    private void CheckGrounded()
    {
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, 0.02f))
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
    }

    private void GrappleOff(ConfigurableJoint jnt)
    {
        jnt.xMotion = ConfigurableJointMotion.Free;
        jnt.yMotion = ConfigurableJointMotion.Free;
        jnt.zMotion = ConfigurableJointMotion.Free;
        jnt.angularXMotion = ConfigurableJointMotion.Free;
        jnt.angularYMotion = ConfigurableJointMotion.Free;
        jnt.angularZMotion = ConfigurableJointMotion.Free;
    }
}
