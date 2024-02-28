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
    private Rigidbody mRigidBody;
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
    private CinemachineFreeLook mainCam;

    private Vector3 movementDirection;
    private bool weMovin;
    private bool jump;
    [SerializeField]
    private bool isGrounded;

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
            mRigidBody.AddForce(Vector3.up * jumpForce);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        CheckGrounded();
        Vector3 facingDirection = mRigidBody.velocity;
        facingDirection.y = 0.0f;
        facingDirection.Normalize();
        gameObject.transform.forward = Vector3.Lerp(gameObject.transform.forward, facingDirection, playerTurnSpeed * Time.deltaTime);

        if (isGrounded)
        {
            if (weMovin)
            {
                Vector3 camJuxtapose = gameObject.transform.position - mainCam.transform.position;
                camJuxtapose.y = 0.0f;
                camJuxtapose.Normalize();

                Quaternion forward = Quaternion.LookRotation(Vector3.forward);
                Quaternion cameraLocalRotation = Quaternion.FromToRotation(Vector3.forward, camJuxtapose);
                Debug.Log(cameraLocalRotation);
                float angleOfRotation = Quaternion.Angle(forward, cameraLocalRotation);
                Vector3 finalMovementDirection = Quaternion.AngleAxis(angleOfRotation, Vector3.Dot(Vector3.right, camJuxtapose) > 0 ? Vector3.up : Vector3.down) * movementDirection;
                mRigidBody.AddForce(finalMovementDirection * movementForce);
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
}