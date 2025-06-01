using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f; // smaller = snappier
    [SerializeField] private float damping = 5f; // higher = slower deceleration
    [SerializeField] private float arrowKeyRotationSpeed = 20f; // Adjusted for arrow key specific speed

    private float currentSpeed;

    private void Update()
    {
        float targetSpeed = 0f;

        // Mouse Input
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetSpeed = -mouseX * rotationSpeed; // Flip direction and apply rotation speed
        }
        // Keyboard Input (Left/Right Arrow Keys ONLY)
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            targetSpeed = arrowKeyRotationSpeed; // Rotate left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            targetSpeed = -arrowKeyRotationSpeed; // Rotate right
        }
        // If no mouse drag or arrow key input, target speed is 0, allowing deceleration
        else
        {
            targetSpeed = 0f;
        }

        // Smoothly interpolate toward target speed when input is active,
        // or smoothly decelerate when input is released.
        if (Mathf.Abs(targetSpeed) > 0.01f) // If there's an active target speed (from mouse or keys)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);
        }
        else // No active input, so decelerate
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * damping);
        }

        // Apply rotation
        transform.Rotate(Vector3.up, currentSpeed * Time.deltaTime, Space.World); // Added Time.deltaTime here
    }
}