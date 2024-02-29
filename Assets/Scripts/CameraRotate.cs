using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotate : MonoBehaviour
{
    [SerializeField]
    private FloatObject cameraSensitivity;
    [SerializeField]
    private FloatObject yClamp;

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();
        float angleX = mouseDelta.x * cameraSensitivity.value * Time.deltaTime;
        float angleY = mouseDelta.y * cameraSensitivity.value * Time.deltaTime;
        gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.AngleAxis(angleX, Vector3.up) * Quaternion.AngleAxis(angleY, Vector3.left);
        Vector3 eA = gameObject.transform.rotation.eulerAngles;
        float transAngle = eA.x;
        Debug.Log(eA.x);
        if (eA.x <= 90.0f && eA.x >= 10.0f)
        {
            transAngle = Mathf.Clamp(eA.x, 0.0f, 90.0f - yClamp.value);
        }
        else if (eA.x >= 270.0f && eA.x <= 350.0f)
        {
            transAngle = Mathf.Clamp(eA.x, 270.0f + yClamp.value, 360.0f);
        }
        Quaternion removeZ = Quaternion.Euler(new Vector3(transAngle, eA.y, 0.0f));
        gameObject.transform.rotation = removeZ;
    }
}
