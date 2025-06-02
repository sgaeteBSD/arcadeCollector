using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f; [SerializeField] private float damping = 5f; [SerializeField] private float arrowKeyRotationSpeed = 20f;
    private float currentSpeed;

    private void Update()
    {
        float targetSpeed = 0f;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetSpeed = -mouseX * rotationSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            targetSpeed = arrowKeyRotationSpeed;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            targetSpeed = -arrowKeyRotationSpeed;
        }
        else
        {
            targetSpeed = 0f;
        }

        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * damping);
        }

        transform.Rotate(Vector3.up, currentSpeed * Time.deltaTime, Space.World);
    }
}