// Scripts/Crane/ClawMechanism.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    [Header("Claw Parts")]
    public Transform clawLeft;
    public Transform clawRight;
    public Collider2D clawLeftCollider;
    public Collider2D clawRightCollider;
    public float clawOpenAngle = 0f;    // 0f is OPEN
    public float clawCloseAngle = 30f;   // 30f is CLOSED
    public float clawMoveSpeed = 5f;
    public float clawDropDistance = 5f; // Max distance claws can drop from crane body
    public LayerMask grabbableLayer;

    public float clawOpenCloseSpeed = 100f; // degrees per second for opening/closing

    [Header("Grab Logic")]
    public float grabStrength = 100f; // For potential joint breaking force (if not using parenting)
    private GameObject currentGrabbedObject = null; // Private tracker for what's currently grabbed

    // Private references for physics components (optimize by getting them once)
    private Rigidbody2D rbLeftClaw;
    private Rigidbody2D rbRightClaw;

    private void Awake()
    {
        // Get Rigidbody2D references once
        rbLeftClaw = clawLeftCollider.GetComponent<Rigidbody2D>();
        rbRightClaw = clawRightCollider.GetComponent<Rigidbody2D>();

        if (rbLeftClaw == null || rbRightClaw == null)
        {
            Debug.LogError("ClawMechanism: Missing Rigidbody2D on claw colliders! Disabling script.");
            enabled = false; // Disable script if essential components are missing
        }
    }

    // Public method to set initial open state, called by CraneController
    public void SetClawOpen()
    {
        SetClawRotation(clawOpenAngle);
    }

    // This coroutine performs the entire drop, grab, and ascent cycle
    // It uses an 'out' parameter to return the grabbed object
    public IEnumerator DropAndGrab(float craneInitialY)
    {
        currentGrabbedObject = null; // Reset for this new drop cycle

        float targetY = craneInitialY - clawDropDistance; // Max depth for the crane's body

        SetClawRotation(clawOpenAngle); // Ensure claws are open
        yield return null; // Allow one frame for rotation to apply

        // --- Phase 1: Descend & Check for Immediate Contact ---
        bool contactMadeDuringDescent = false;
        while (transform.position.y > targetY && !contactMadeDuringDescent)
        {
            transform.position += Vector3.down * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders(); // Keep colliders synced with crane's position

            if (CheckClawContact())
            {
                contactMadeDuringDescent = true;
                Debug.Log("Claw made contact during descent. Proceeding to grab attempt.");
                // We don't break here, we continue to the close/grab phase immediately
            }
            yield return null;
        }

        // Ensure crane reaches its target Y if no contact was made
        if (!contactMadeDuringDescent)
        {
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            UpdateClawColliders();
        }

        yield return new WaitForSeconds(0.2f); // Brief pause before closing

        // --- Phase 2: Close Claws and Attempt Grab ---
        float currentClawAngle = clawOpenAngle;
        while (currentClawAngle < clawCloseAngle)
        {
            currentClawAngle += clawOpenCloseSpeed * Time.deltaTime;
            currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
            SetClawRotation(currentClawAngle);

            // Attempt to grab if contact is made and we haven't grabbed already
            if (CheckClawContact() && currentGrabbedObject == null)
            {
                // Find all overlapping colliders around the center of the crane's current position
                // Adjust radius based on your claw's size and grab area
                Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 0.75f, grabbableLayer);
                if (overlappingColliders.Length > 0)
                {
                    // For simplicity, grab the first detected object.
                    // You might want more sophisticated logic here (e.g., closest, largest).
                    TryGrabObject(overlappingColliders[0].gameObject);
                    if (currentGrabbedObject != null)
                    {
                        Debug.Log($"Successfully grabbed: {currentGrabbedObject.name}");
                        break; // If grabbed, stop closing and proceed
                    }
                }
            }
            yield return null;
        }

        // Ensure claws are fully closed or at the point of grab, before going up
        SetClawRotation(currentClawAngle); // Final snap to closed/grab angle
        yield return new WaitForSeconds(0.3f); // Brief pause after closing/grab attempt

        // --- Phase 3: Ascend with or without grabbed object ---
        while (transform.position.y < craneInitialY)
        {
            transform.position += Vector3.up * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders(); // Keep colliders synced
            // If an object is grabbed, it will automatically follow due to parenting
            yield return null;
        }

        // Ensure crane is at the exact starting height
        transform.position = new Vector3(transform.position.x, craneInitialY, transform.position.z);

        // --- Phase 4: Open Claws and Signal Grabbed Object ---
        float currentOpenAngle = clawCloseAngle; // Start from closed
        while (currentOpenAngle > clawOpenAngle)
        {
            currentOpenAngle -= clawOpenCloseSpeed * Time.deltaTime;
            currentOpenAngle = Mathf.Max(currentOpenAngle, clawOpenAngle);
            SetClawRotation(currentOpenAngle);
            yield return null;
        }
        SetClawRotation(clawOpenAngle); // Ensure fully open

        // Return the grabbed object reference
        yield return currentGrabbedObject;

        // Important: At this point, the item is still parented to the crane.
        // The CraneController will now decide what to do with it (e.g., move to chute, then call ReleaseGrabbedObject).
        // If the item needs to be immediately released to fall, call ReleaseGrabbedObject(grabbedItem) here.
        // For now, I've left the `ReleaseGrabbedObject` call in CraneController for the modular flow.
    }

    // Sets the rotation of the individual claw blades relative to the crane body
    private void SetClawRotation(float angle)
    {
        // These are local rotations because clawLeft/Right are children of the main CraneController
        // if you haven't introduced the ClawRotationPivot GameObject.
        // If they are children of ClawRotationPivot, this would apply to the pivot, not individual claws.
        // Assuming clawLeft and clawRight are children of the main CraneController here:
        clawLeft.localRotation = Quaternion.Euler(0, 0, angle);
        clawRight.localRotation = Quaternion.Euler(0, 0, -angle);
        UpdateClawColliders(); // Sync Rigidbody2D positions after rotation
    }

    // Syncs the Rigidbody2D positions with their Transform positions
    private void UpdateClawColliders()
    {
        // Use the cached Rigidbody2D references
        if (rbLeftClaw != null)
            rbLeftClaw.MovePosition(clawLeft.position);

        if (rbRightClaw != null)
            rbRightClaw.MovePosition(clawRight.position);
    }

    // Checks if either claw collider is touching anything on the grabbableLayer
    private bool CheckClawContact()
    {
        return Physics2D.IsTouchingLayers(clawLeftCollider, grabbableLayer) ||
               Physics2D.IsTouchingLayers(clawRightCollider, grabbableLayer);
    }

    // Handles parenting the grabbed object to the crane
    private void TryGrabObject(GameObject objToGrab)
    {
        if (objToGrab == null || ((1 << objToGrab.layer) & grabbableLayer) == 0) return;

        Rigidbody2D objRb = objToGrab.GetComponent<Rigidbody2D>();
        if (objRb != null)
        {
            currentGrabbedObject = objToGrab;
            // Parent to this ClawMechanism's GameObject (which is the CraneController)
            currentGrabbedObject.transform.SetParent(this.transform);
            objRb.isKinematic = true; // Stop physics simulation for grabbed object
        }
    }

    // Public method to release a grabbed object, called by CraneController or other logic
    public void ReleaseGrabbedObject(GameObject objToRelease)
    {
        if (objToRelease != null)
        {
            objToRelease.transform.SetParent(null); // Detach from crane
            Rigidbody2D objRb = objToRelease.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // Re-enable physics simulation
                objRb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); // Small upward push
            }
            // If the released object was the one currently tracked, clear the reference
            if (objToRelease == currentGrabbedObject)
            {
                currentGrabbedObject = null;
            }
        }
    }
}