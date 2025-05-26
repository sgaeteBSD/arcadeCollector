using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f; // smaller = snappier
    [SerializeField] private float damping = 5f; // higher = slower deceleration

    private float currentVelocity;
    private float currentSpeed;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");

            // Flip the direction and apply rotation speed
            float targetSpeed = -mouseX * rotationSpeed;

            // Smoothly interpolate toward target speed
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);
        }
        else
        {
            // Decelerate smoothly when not dragging
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * damping);
        }

        // Apply rotation
        transform.Rotate(Vector3.up, currentSpeed, Space.World);
    }
}
