using UnityEngine;

public class QEMover : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera Rotation")]
    public float rotateSpeed = 90f;
    public Camera attachedCamera;

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
    }

    void HandleMovement()
    {
        bool qDown = Input.GetKey(KeyCode.Q);
        bool eDown = Input.GetKey(KeyCode.E);

        if (qDown && eDown)
        {
            Vector3 cameraForward = attachedCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            transform.Translate(cameraForward * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    void HandleCameraRotation()
    {
        if (attachedCamera == null) return;

        if (Input.GetKey(KeyCode.A))
        {
            attachedCamera.transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            attachedCamera.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}
