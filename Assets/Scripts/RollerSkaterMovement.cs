using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private Vector2 movementDirection;
    private bool weMovin;
    private bool jump;
    [SerializeField]
    private bool isGrounded;

    public void OnMove(CallbackContext input)
    {
        movementDirection = input.ReadValue<Vector2>();
        if(input.phase != InputActionPhase.Canceled)
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
    }

    void Start()
    {
        
    }

    void Update()
    {
        CheckGrounded();
        Vector3 forward = new Vector3(mRigidBody.velocity.x, 0.0f, mRigidBody.velocity.y);
        gameObject.transform.forward = Vector3.Lerp(gameObject.transform.forward, forward, playerTurnSpeed * Time.deltaTime);

        if (isGrounded)
        {
            if (weMovin)
            {
                mRigidBody.AddForce(new Vector3(movementDirection.y, 0.0f, movementDirection.x) * movementForce);
            }

            if (jump)
            {
                mRigidBody.AddForce(Vector3.up * jumpForce);
            }
        }
    }

    private void CheckGrounded()
    {
        if (Physics.Raycast(gameObject.transform.position, -Vector3.up, 0.01f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
