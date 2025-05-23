// Scripts/Crane/ClawMechanism.cs // Assuming this is ClawMechanism logic, despite the class name.
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
    public float clawOpenAngle = -35f;    // -35f is OPEN
    public float clawCloseAngle = 7f;   // 7f is CLOSED
    public float clawMoveSpeed = 5f; // Vertical speed (descent/ascent)
    public float clawDropDistance = 5f; // Max distance claws can drop from crane body
    public LayerMask grabbableLayer;

    public float clawOpenCloseSpeed = 100f; // degrees per second for opening/closing

    [Header("Grab Logic")]
    public float grabStrength = 100f; // For potential joint breaking force (if not using parenting)
    private GameObject currentGrabbedObject = null; // Private tracker for what's currently grabbed

    // Private references for physics components (optimize by getting them once)
    private Rigidbody2D rbLeftClaw;
    private Rigidbody2D rbRightClaw;

    private bool leftTouch = false;
    private bool rightTouch = false;

    private void Awake()
    {
        // Get Rigidbody2D references once
        rbLeftClaw = clawLeftCollider.GetComponent<Rigidbody2D>();
        rbRightClaw = clawRightCollider.GetComponent<Rigidbody2D>();

        if (rbLeftClaw == null || rbRightClaw == null)
        {
            Debug.LogError("ClawController: Missing Rigidbody2D on claw colliders! Disabling script.");
            enabled = false; // Disable script if essential components are missing
        }
    }

    // Public method to set initial open state, called by CraneController
    public void SetClawOpen()
    {
        SetClawRotation(clawOpenAngle);
    }

    // This coroutine performs the entire drop, grab, and ascent cycle
    // It is now expected to also handle the horizontal prize delivery movement
    // and continuous grab attempts during that phase.
    public IEnumerator DropAndGrab(float craneInitialY, float targetDropX, float craneMoveSpeed) // Added targetDropX and craneMoveSpeed
    {
        currentGrabbedObject = null; // Reset for this new drop cycle
        leftTouch = false;
        rightTouch = false;

        float targetY = craneInitialY - clawDropDistance; // Max depth for the crane's body
        Vector3 initialCranePos = transform.position; // Store initial position for return

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
        // This loop will now potentially continue into the horizontal movement phase
        float currentClawAngle = clawOpenAngle;
        bool grabAttemptFinished = false;

        while (currentClawAngle < clawCloseAngle || !grabAttemptFinished)
        {
            // Only try to close if not fully closed yet
            if (currentClawAngle < clawCloseAngle)
            {
                currentClawAngle += clawOpenCloseSpeed * Time.deltaTime;
                currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
                SetClawRotation(currentClawAngle);
            }

            leftTouch = clawLeftCollider.IsTouchingLayers(grabbableLayer);
            rightTouch = clawRightCollider.IsTouchingLayers(grabbableLayer);

            // Attempt to grab if both are touching and we haven't grabbed yet
            if (leftTouch && rightTouch && currentGrabbedObject == null)
            {
                // Find all overlapping colliders around the center of the crane's current position
                Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 0.25f, grabbableLayer);
                if (overlappingColliders.Length > 0)
                {
                    TryGrabObject(overlappingColliders[0].gameObject);
                    if (currentGrabbedObject != null)
                    {
                        Debug.Log($"Successfully grabbed: {currentGrabbedObject.name}");
                        // Once grabbed, we no longer need to keep trying to grab
                        grabAttemptFinished = true;
                    }
                }
            }
            // If claws are fully closed and we haven't grabbed anything, stop trying to grab
            else if (currentClawAngle >= clawCloseAngle && currentGrabbedObject == null)
            {
                grabAttemptFinished = true;
            }

            // If we've finished the grab attempt (either grabbed or fully closed without grab)
            // and the claws are fully closed, we can break this specific closing phase.
            // BUT, if we are grabbing, we want to keep them closed.
            if (grabAttemptFinished && currentClawAngle >= clawCloseAngle && currentGrabbedObject != null)
            {
                // We've grabbed, and the claws are fully closed around it.
                // This phase is complete.
                break;
            }
            // If fully closed and didn't grab, also move on
            if (currentClawAngle >= clawCloseAngle && currentGrabbedObject == null)
            {
                break;
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

            // Continuously try to close claws if an object is grabbed (maintain grip)
            if (currentGrabbedObject != null && currentClawAngle < clawCloseAngle)
            {
                currentClawAngle += clawOpenCloseSpeed * Time.deltaTime;
                currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
                SetClawRotation(currentClawAngle);
            }

            yield return null;
        }

        // Ensure crane is at the exact starting height
        transform.position = new Vector3(transform.position.x, craneInitialY, transform.position.z);
        Vector3 targetDropPosition = new Vector3(targetDropX, transform.position.y, transform.position.z);

        // --- NEW Phase 4: Horizontal Movement to Drop Point with Continuous Grab Checks ---
        if (currentGrabbedObject != null) // Only move if an item was grabbed
        {
            Debug.Log($"Grabbed {currentGrabbedObject.name}. Moving to prize drop zone.");

            while (Vector3.Distance(transform.position, targetDropPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetDropPosition, craneMoveSpeed * Time.deltaTime);
                UpdateClawColliders(); // Keep colliders synced during horizontal move

                // Continuously try to keep claws closed and maintain grip
                // This will re-parent if the object somehow gets unparented
                // or ensure it stays kinematic.
                if (currentGrabbedObject != null)
                {
                    // Ensure the object is still parented and kinematic
                    if (currentGrabbedObject.transform.parent != this.transform ||
                        (currentGrabbedObject.GetComponent<Rigidbody2D>() != null && !currentGrabbedObject.GetComponent<Rigidbody2D>().isKinematic))
                    {
                        TryGrabObject(currentGrabbedObject); // Re-grab if somehow lost
                        if (currentGrabbedObject != null) // Re-check after re-grab attempt
                        {
                            // If we're still holding, keep the claws trying to close
                            if (currentClawAngle < clawCloseAngle)
                            {
                                currentClawAngle += clawOpenCloseSpeed * Time.deltaTime;
                                currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
                                SetClawRotation(currentClawAngle);
                            }
                        }
                    }
                }
                else // If grabbed object is null, but it was supposed to be grabbed
                {
                    // This means the object was lost during transport.
                    // You might want to stop the transport and open claws here, or just let it drop.
                    Debug.Log("Lost object during transport!");
                    break; // Stop moving if object is lost
                }

                yield return null; // Wait for next frame
            }
            transform.position = targetDropPosition; // Snap to exact target if loop finished
            UpdateClawColliders(); // Final sync

            Debug.Log($"Reached drop zone. Releasing {currentGrabbedObject.name}.");
            ReleaseGrabbedObject(currentGrabbedObject); // Release the item
            yield return new WaitForSeconds(0.5f); // Short pause to see the drop
        }
        else
        {
            Debug.Log("No item grabbed. Skipping prize delivery move.");
            transform.position = Vector3.MoveTowards(transform.position, targetDropPosition, craneMoveSpeed * Time.deltaTime);
        }
        // --- END NEW PRIZE DELIVERY PHASE ---

        // --- Phase 5: Open Claws and Return to Initial X Position ---
        // Open claws after release (or if nothing was grabbed)
        float currentOpenAngle = clawCloseAngle; // Start from closed
        while (currentOpenAngle > clawOpenAngle)
        {
            currentOpenAngle -= clawOpenCloseSpeed * Time.deltaTime;
            currentOpenAngle = Mathf.Max(currentOpenAngle, clawOpenAngle);
            SetClawRotation(currentOpenAngle);
            yield return null;
        }
        SetClawRotation(clawOpenAngle); // Ensure fully open

        
        UpdateClawColliders(); // Final sync

        // Return the grabbed object reference (will be null if released or not grabbed)
        yield return currentGrabbedObject; // This will now be null if successfully released
    }

    // Sets the rotation of the individual claw blades relative to the crane body
    public void SetClawRotation(float angle)
    {
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
            //currentGrabbedObject = objToGrab;
            //currentGrabbedObject.transform.SetParent(this.transform); // Parent to this ClawController
            //objRb.isKinematic = true; // Stop physics simulation for grabbed object
           // Debug.Log($"Grabbed: {objToGrab.name}, Parented to {this.name}, Kinematic: {objRb.isKinematic}");
        }
    }

    // Public method to release a grabbed object, called by CraneController or other logic
    public void ReleaseGrabbedObject(GameObject objToRelease)
    {
        if (objToRelease != null)
        {
            Debug.Log($"Attempting to release {objToRelease.name}");
            objToRelease.transform.SetParent(null); // Detach from crane
            Rigidbody2D objRb = objToRelease.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // Re-enable physics simulation
                objRb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); // Small upward push
                Debug.Log($"{objToRelease.name} released, Kinematic: {objRb.isKinematic}");
            }
            // If the released object was the one currently tracked, clear the reference
            if (objToRelease == currentGrabbedObject)
            {
                currentGrabbedObject = null;
            }
        }
    }
}