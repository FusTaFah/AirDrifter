using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotate : MonoBehaviour
{
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();
        gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.AngleAxis(mouseDelta.x * Time.deltaTime, Vector3.up) * Quaternion.AngleAxis(-mouseDelta.y * Time.deltaTime, Vector3.right);
    }
}
