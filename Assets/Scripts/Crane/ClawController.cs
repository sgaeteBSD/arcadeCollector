// Scripts/Crane/ClawController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    [Header("Claw Parts")]
    public Transform clawLeft;  // The Transform for the left claw's visual and pivot
    public Transform clawRight; // The Transform for the right claw's visual and pivot
    public Collider2D clawLeftCollider; // The Collider2D on the left claw
    public Collider2D clawRightCollider; // The Collider2D on the right claw
    public float clawOpenAngle = 0f;    // 0f is OPEN
    public float clawCloseAngle = 30f;   // 30f is CLOSED
    public float clawDropDistance = 5f;
    public LayerMask grabbableLayer;

    [Header("Claw Speeds")]
    public float clawVerticalSpeed = 5f; // Vertical speed of the claws during descent/ascent
    public float clawOpenCloseSpeed = 100f; // degrees per second for opening/closing

    [Header("Angled Grab")]
    public float rotationSpeed = 100f; // Speed at which the claws rotate for angled grab
    public float maxAngleDeviation = 45f; // Max angle one claw can move independently

    [Header("Grab Logic")]
    public float grabStrength = 100f; // For potential joint breaking force (not used in current parenting method)

    // Private references for physics
    private Rigidbody2D rbLeftClaw;
    private Rigidbody2D rbRightClaw;

    private Vector3 initialClawLeftLocalPos; // Store initial local positions relative to pivot
    private Vector3 initialClawRightLocalPos;


    private void Awake()
    {
        // Get Rigidbody2D references once
        rbLeftClaw = clawLeftCollider.GetComponent<Rigidbody2D>();
        rbRightClaw = clawRightCollider.GetComponent<Rigidbody2D>();

        if (rbLeftClaw == null || rbRightClaw == null)
        {
            Debug.LogError("ClawController: Missing Rigidbody2D on claw colliders!");
            enabled = false; // Disable script if essential components are missing
            return;
        }

        // Store initial local positions for resetting (assuming they are children of this GameObject)
        initialClawLeftLocalPos = clawLeft.localPosition;
        initialClawRightLocalPos = clawRight.localPosition;
    }

    // Public method to be called by CraneController for initial state
    public void SetClawInitialState()
    {
        // Reset pivot rotation
        transform.localRotation = Quaternion.identity;
        // Reset individual claw positions (they should be children of this pivot)
        clawLeft.localPosition = initialClawLeftLocalPos;
        clawRight.localPosition = initialClawRightLocalPos;
        SetClawRotation(clawOpenAngle);
    }


    // Public method to be called by CraneController to start the drop
    public IEnumerator DropClaws()
    {
        GameObject currentGrabbedObject = null; // Local variable for the coroutine

        // Store the initial Y position of the pivot (this GameObject)
        float initialPivotY = transform.position.y;
        float targetPivotY = initialPivotY - clawDropDistance;

        // Reset individual claw positions (relative to this pivot) and rotation
        SetClawInitialState();
        UpdateClawColliders(); // Ensure rigidbodies match transform

        // --- Phase 1: Descend ---
        bool leftClawHit = false;
        bool rightClawHit = false;

        while (!leftClawHit || !rightClawHit) // Keep dropping until BOTH claws hit something
        {
            // Move self (pivot) down if not yet at targetY and no claw has stopped it
            float currentPivotY = transform.position.y;
            bool reachedMinY = currentPivotY <= targetPivotY;

            if (!reachedMinY)
            {
                // Only move pivot if neither claw has hit, or both have hit AND the pivot needs to catch up
                // (This ensures the pivot correctly follows the lowest moving claw)
                float moveAmount = clawVerticalSpeed * Time.deltaTime;
                transform.position += Vector3.down * moveAmount;
            }


            // Update claw positions to follow the pivot's new Y position
            // We're keeping their local Y fixed, so they just move with the parent
            // But we need to check if they hit something.

            // Raycast for precision stopping (more reliable than IsTouchingLayers for stopping at contact)
            float raycastOriginYOffset = initialClawLeftLocalPos.y; // Y offset from pivot to claw's center
            // Need to adjust this based on the actual shape of your collider
            // For a downward ray from the lowest point of the claw, you'd calculate:
            // float lowestPointOfClawColliderY = clawLeft.position.y - (clawLeftCollider.bounds.size.y / 2f);
            // Vector2 raycastOrigin = new Vector2(clawLeft.position.x, lowestPointOfClawColliderY);
            // For simplicity, we'll use claw's current position and a small offset
            float raycastLookAhead = clawVerticalSpeed * Time.deltaTime + 0.05f; // Look slightly ahead of movement

            // Left Claw logic
            if (!leftClawHit)
            {
                RaycastHit2D hitLeft = Physics2D.Raycast(clawLeft.position, Vector2.down, raycastLookAhead, grabbableLayer);
                Debug.DrawRay(clawLeft.position, Vector2.down * raycastLookAhead, hitLeft ? Color.green : Color.red);

                if (hitLeft.collider != null)
                {
                    leftClawHit = true;
                    // Adjust claw's position to stop exactly at hit point
                    clawLeft.position = new Vector3(clawLeft.position.x, hitLeft.point.y + (clawLeftCollider.bounds.size.y / 2f) + 0.01f, clawLeft.position.z);
                    Debug.Log($"Left claw hit {hitLeft.collider.name} at Y: {hitLeft.point.y}");
                }
            }

            // Right Claw logic
            if (!rightClawHit)
            {
                RaycastHit2D hitRight = Physics2D.Raycast(clawRight.position, Vector2.down, raycastLookAhead, grabbableLayer);
                Debug.DrawRay(clawRight.position, Vector2.down * raycastLookAhead, hitRight ? Color.green : Color.red);

                if (hitRight.collider != null)
                {
                    rightClawHit = true;
                    // Adjust claw's position to stop exactly at hit point
                    clawRight.position = new Vector3(clawRight.position.x, hitRight.point.y + (clawRightCollider.bounds.size.y / 2f) + 0.01f, clawRight.position.z);
                    Debug.Log($"Right claw hit {hitRight.collider.name} at Y: {hitRight.point.y}");
                }
            }

            // Move the pivot down *unless* both claws have hit AND the pivot is not yet at its lowest required point
            // This is complex. Let's simplify: the pivot continues until its lowest claw has hit or it hits its max drop.
            if (!leftClawHit || !rightClawHit)
            {
                // Keep moving the pivot (this GameObject) down
                // We don't directly control clawLeft.position and clawRight.position here,
                // they are children and move with the parent.
                // We only adjust their Y if they hit something.
                if (transform.position.y > targetPivotY)
                {
                    transform.position += Vector3.down * clawVerticalSpeed * Time.deltaTime;
                    // After moving the pivot, re-check if any claw hit something *because of this new pivot position*
                    // This creates a challenge for the "one claw stops, other keeps going" logic
                    // A better way is for the *pivot itself* to stop if its lowest claw hit.
                }
            }
            // SIMPLIFIED APPROACH for descent: Pivot moves down. If a claw hits, it sets its Y position to the hit point,
            // and the pivot continues to move. The other claw keeps going with the pivot.
            // This allows the pivot to move past the stopped claw's Y.
            // The angled grab will then be a rotation around the pivot.


            // Update the physics rigidbodies to match their transforms
            UpdateClawColliders();
            yield return null;

            // Re-evaluate loop condition after moving and potential hits
            if (transform.position.y <= targetPivotY && leftClawHit && rightClawHit) break; // All done
            if (transform.position.y <= targetPivotY && !leftClawHit && !rightClawHit) break; // Reached bottom without hitting
        }

        // Ensure pivot reached bottom if nothing hit
        transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y, targetPivotY), transform.position.z);
        UpdateClawColliders(); // Final update after descent
        yield return new WaitForSeconds(0.2f); // Brief pause after descent

        // --- Phase 2: Angled Grab Rotation (if applicable) ---
        // This only happens if there was a difference in Y positions *before* final closure
        if (Mathf.Abs(clawLeft.position.y - clawRight.position.y) > 0.05f) // Check for significant Y difference
        {
            Debug.Log("Performing angled grab rotation.");
            // Determine which claw is higher (the one that hit first)
            Transform higherClaw = (clawLeft.position.y > clawRight.position.y) ? clawLeft : clawRight;
            Transform lowerClaw = (clawLeft.position.y < clawRight.position.y) ? clawRight : clawLeft;

            float currentYDifference = Mathf.Abs(clawLeft.position.y - clawRight.position.y);
            float currentClawArmLength = Mathf.Abs(initialClawLeftLocalPos.x - initialClawRightLocalPos.x); // Distance between claw pivots
            currentClawArmLength = Mathf.Max(currentClawArmLength, 0.1f); // Avoid division by zero

            // Calculate target pivot rotation angle
            // This assumes the claws are rigid and rotate around the pivot
            float targetPivotAngle = Mathf.Atan2(currentYDifference, currentClawArmLength) * Mathf.Rad2Deg;
            targetPivotAngle = Mathf.Min(targetPivotAngle, maxAngleDeviation); // Clamp to max allowed deviation

            // If left claw is higher, pivot rotates positively (clockwise from Unity's default 2D perspective)
            // If right claw is higher, pivot rotates negatively
            float finalPivotAngle = (clawLeft.position.y > clawRight.position.y) ? targetPivotAngle : -targetPivotAngle;

            Quaternion initialPivotRotation = transform.localRotation;
            Quaternion targetPivotRotation = Quaternion.Euler(0, 0, finalPivotAngle);

            float rotateTimer = 0f;
            float rotationDuration = 0.5f; // Adjust how long it takes to rotate

            while (rotateTimer < rotationDuration)
            {
                transform.localRotation = Quaternion.Slerp(initialPivotRotation, targetPivotRotation, rotateTimer / rotationDuration);
                UpdateClawColliders(); // Ensure colliders update with pivot rotation
                rotateTimer += Time.deltaTime;
                yield return null;
            }
            transform.localRotation = targetPivotRotation; // Ensure it snaps to final angle
            UpdateClawColliders();
        }

        // --- Phase 3: Close Claws ---
        float currentClawAngle = clawOpenAngle; // This is the individual claw blade angle
        while (currentClawAngle < clawCloseAngle)
        {
            currentClawAngle += clawOpenCloseSpeed * Time.deltaTime;
            currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
            SetClawRotation(currentClawAngle); // This rotates the individual claw transforms

            // Check for grab contact during closing
            if (CheckTotalClawContact()) // Use the combined check here
            {
                // Find all overlapping colliders and try to grab one (e.g., the closest)
                Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 1f, grabbableLayer); // Use pivot position, larger radius
                if (overlappingColliders.Length > 0)
                {
                    // Simple grab: take the first one found
                    currentGrabbedObject = TryGrabObject(overlappingColliders[0].gameObject);
                    if (currentGrabbedObject != null)
                    {
                        Debug.Log($"Successfully grabbed: {currentGrabbedObject.name}");
                        break; // Stop closing if successfully grabbed
                    }
                }
            }
            yield return null;
        }

        // Ensure claws are fully closed or at the point of grab
        SetClawRotation(currentClawAngle); // Final snap to closed angle if not broken by grab

        yield return new WaitForSeconds(0.3f); // Brief pause after closing/grab attempt

        // --- Phase 4: Ascend ---
        Vector3 initialAscentPivotPos = transform.position; // Position of the pivot at start of ascent
        float ascentTargetY = initialPivotY; // Target Y for the pivot to return to

        while (transform.position.y < ascentTargetY)
        {
            float moveAmount = clawVerticalSpeed * Time.deltaTime;
            transform.position += Vector3.up * moveAmount; // Move the pivot (this GameObject) up

            UpdateClawColliders(); // Update colliders as pivot moves

            // Grabbed object automatically follows because it's parented to this pivot (if Option A)
            yield return null;
        }

        // Ensure pivot is at the exact starting height
        transform.position = new Vector3(transform.position.x, ascentTargetY, transform.position.z);
        UpdateClawColliders(); // Final update

        // --- Phase 5: Open Claws and Reset Rotation ---
        float currentOpenAngle = clawCloseAngle; // Start from closed
        while (currentOpenAngle > clawOpenAngle)
        {
            currentOpenAngle -= clawOpenCloseSpeed * Time.deltaTime; // Use same speed for open/close
            currentOpenAngle = Mathf.Max(currentOpenAngle, clawOpenAngle);
            SetClawRotation(currentOpenAngle);
            yield return null;
        }
        SetClawRotation(clawOpenAngle); // Ensure fully open

        // Reset claw pivot rotation to zero
        transform.localRotation = Quaternion.identity;
        // Reset individual claw local positions (they are children, so this aligns them)
        clawLeft.localPosition = initialClawLeftLocalPos;
        clawRight.localPosition = initialClawRightLocalPos;
        UpdateClawColliders();

        // Pass the grabbed object back to the CraneController
        yield return currentGrabbedObject;
    }

    // Controls the rotation of the individual claw blades
    private void SetClawRotation(float angle)
    {
        // These are local rotations because clawLeft/Right are children of this pivot
        clawLeft.localRotation = Quaternion.Euler(0, 0, angle);
        clawRight.localRotation = Quaternion.Euler(0, 0, -angle);
        UpdateClawColliders(); // Important to update collider positions after rotation
    }

    // Syncs the Rigidbody2D positions with their Transform positions
    private void UpdateClawColliders()
    {
        if (rbLeftClaw != null) rbLeftClaw.MovePosition(clawLeft.position);
        if (rbRightClaw != null) rbRightClaw.MovePosition(clawRight.position);
    }

    // Checks contact for a single specified claw collider
    private bool CheckIndividualClawContact(Collider2D clawCollider)
    {
        return Physics2D.IsTouchingLayers(clawCollider, grabbableLayer);
    }

    // Checks if either claw is touching anything grabbable
    private bool CheckTotalClawContact()
    {
        return Physics2D.IsTouchingLayers(clawLeftCollider, grabbableLayer) ||
               Physics2D.IsTouchingLayers(clawRightCollider, grabbableLayer);
    }

    // Handles grabbing an object (used by DropClaws coroutine)
    private GameObject TryGrabObject(GameObject objToGrab)
    {
        if (objToGrab == null || ((1 << objToGrab.layer) & grabbableLayer) == 0) return null;

        Rigidbody2D objRb = objToGrab.GetComponent<Rigidbody2D>();
        if (objRb != null)
        {
            objToGrab.transform.SetParent(this.transform); // Parent to this ClawController (the pivot)
            objRb.isKinematic = true;
            return objToGrab;
        }
        return null;
    }

    // Handles releasing a grabbed object (called by CraneController)
    public void ReleaseGrabbedObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.SetParent(null); // Detach from crane
            Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // Let it fall/react to physics again
                objRb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); // Optional: small push
            }
            // Optional: Revert squish visual if applied
        }
    }
}