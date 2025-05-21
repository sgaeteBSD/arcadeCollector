using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Assuming this is still used for PlayDisplayManager if not modularized

public class CraneController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float leftLimit = -7f;
    public float rightLimit = 7f;

    [Header("Plays")]
    public int maxPlays = 5;
    private int currentPlays;
    public GameObject[] playNumberDisplays; // Assign your PlayNumber_0, PlayNumber_1, ..., PlayNumber_5 GameObjects here

    [Header("Claw Parts")]
    public Transform clawLeft;
    public Transform clawRight;
    public Collider2D clawLeftCollider;
    public Collider2D clawRightCollider;
    public float clawOpenAngle = 0f;    // NOW: 0f is OPEN
    public float clawCloseAngle = 30f;   // NOW: 30f is CLOSED
    public float clawMoveSpeed = 5f;
    public float clawDropDistance = 5f;
    public LayerMask grabbableLayer;

    public float clawOpenSpeed = 100f; // degrees per second
    public float clawCloseSpeed = 100f; // Added for clarity

    [Header("Grab Logic")]
    public float grabStrength = 100f; // How much force or how "sticky" the grab is. Adjust this.
    private GameObject grabbedObject = null; // Reference to the object currently grabbed


    private Vector3 startPosition;
    private bool isDropping = false;

    private void Start()
    {
        startPosition = transform.position;
        currentPlays = maxPlays;
        UpdatePlayDisplay(); // Call this to set the initial display
        SetClawRotation(clawOpenAngle); // Ensure claws are open at game start
    }

    private void Update()
    {
        if (isDropping || currentPlays <= 0) return; // Allow interaction if plays are 0 for a moment to see the '0' display

        float h = Input.GetAxisRaw("Horizontal");
        transform.position += Vector3.right * h * moveSpeed * Time.deltaTime;
        ClampPosition();

        // Prevent dropping if currentPlays is already 0, but still allow movement to see the "0" display
        if (Input.GetKeyDown(KeyCode.Space) && currentPlays > 0)
        {
            StartCoroutine(DropClaw());
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        transform.position = pos;
    }

    private IEnumerator DropClaw()
    {
        isDropping = true;
        grabbedObject = null; // Reset grabbed object at the start of a new drop
        float targetY = transform.position.y - clawDropDistance;
        Vector3 startPos = transform.position;

        SetClawRotation(clawOpenAngle);
        yield return null;

        // --- Phase 1: Descend & Attempt Immediate Grab on Contact ---
        bool contactMadeDuringDescent = false;
        while (transform.position.y > targetY && !contactMadeDuringDescent)
        {
            transform.position += Vector3.down * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders();

            // Check if any claw makes contact during descent
            if (CheckClawContact())
            {
                contactMadeDuringDescent = true;
                Debug.Log("Claw made contact during descent. Attempting grab!");
                // No 'break' here, we want to continue to the closing phase immediately
            }

            yield return null;
        }

        // Ensure we stop precisely at the targetY if no contact was made
        if (!contactMadeDuringDescent)
        {
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            UpdateClawColliders();
        }

        yield return new WaitForSeconds(0.2f); // Brief pause before closing

        // --- Phase 2: Close Claws (whether contact was made or not) ---
        float currentClawAngle = clawOpenAngle; // Initialize correctly
        while (currentClawAngle < clawCloseAngle)
        {
            currentClawAngle += clawCloseSpeed * Time.deltaTime;
            currentClawAngle = Mathf.Min(currentClawAngle, clawCloseAngle);
            SetClawRotation(currentClawAngle);

            // Attempt to grab if contact is made during closing
            if (CheckClawContact() && grabbedObject == null) // Only try to grab if not already grabbed
            {
                Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f, grabbableLayer); // Use crane's position, adjust radius
                if (overlappingColliders.Length > 0)
                {
                    // Attempt to grab the first object found
                    TryGrabObject(overlappingColliders[0].gameObject);
                    if (grabbedObject != null)
                    {
                        Debug.Log($"Successfully grabbed: {grabbedObject.name}");
                        // If grabbed, we can break out of the closing loop if we want a quick grab
                        // Or allow it to fully close to ensure a tight grip
                        // For "try and grab first", we'll break here
                        break;
                    }
                }
            }
            yield return null;
        }

        // Ensure claws are fully closed or at the point of grab
        SetClawRotation(currentClawAngle); // Final snap to closed angle if not broken by grab

        yield return new WaitForSeconds(0.3f); // Brief pause after closing/grab attempt

        // --- Phase 3: Ascend ---
        while (transform.position.y < startPos.y)
        {
            transform.position += Vector3.up * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders();
            // Make grabbed object follow crane during ascent (if parented)
            // If using the parenting method, it will automatically follow.
            yield return null;
        }

        // Ensure we are back at the exact starting height
        transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);

        // --- Phase 4: Open Claws and Release Grabbed Object ---
        float currentOpenAngle = clawCloseAngle; // Start from closed
        while (currentOpenAngle > clawOpenAngle)
        {
            currentOpenAngle -= clawOpenSpeed * Time.deltaTime;
            currentOpenAngle = Mathf.Max(currentOpenAngle, clawOpenAngle);
            SetClawRotation(currentOpenAngle);
            yield return null;
        }
        SetClawRotation(clawOpenAngle); // Ensure fully open

        // Release the grabbed object here
        if (grabbedObject != null)
        {
            ReleaseGrabbedObject();
            Debug.Log($"Released: {grabbedObject.name}");
        }

        // --- Final Cleanup ---
        currentPlays--;
        UpdatePlayDisplay(); // Update the GameObject display

        isDropping = false;

        if (currentPlays <= 0)
        {
            Debug.Log("Out of plays. Reset level!");
            // TODO: Reset or end game logic (e.g., show game over screen, disable controls)
        }
    }

    private void SetClawRotation(float angle)
    {
        clawLeft.localRotation = Quaternion.Euler(0, 0, angle);
        clawRight.localRotation = Quaternion.Euler(0, 0, -angle);
        UpdateClawColliders();
    }

    private void UpdateClawColliders()
    {
        // Get Rigidbody2D references once if you were to optimize, but this works
        // for individual calls if components are always present.
        Rigidbody2D rbLeft = clawLeftCollider.GetComponent<Rigidbody2D>();
        Rigidbody2D rbRight = clawRightCollider.GetComponent<Rigidbody2D>();

        if (rbLeft != null)
            rbLeft.MovePosition(clawLeft.position);

        if (rbRight != null)
            rbRight.MovePosition(clawRight.position);
    }

    private bool CheckClawContact()
    {
        return Physics2D.IsTouchingLayers(clawLeftCollider, grabbableLayer) ||
               Physics2D.IsTouchingLayers(clawRightCollider, grabbableLayer);
    }

    private void UpdatePlayDisplay()
    {
        // First, disable all play number GameObjects
        foreach (GameObject display in playNumberDisplays)
        {
            if (display != null)
            {
                display.SetActive(false);
            }
        }

        // Calculate the correct index.
        int indexToShow = Mathf.Clamp(currentPlays, 0, playNumberDisplays.Length - 1);

        // Enable only the GameObject corresponding to the current number of plays
        if (indexToShow >= 0 && indexToShow < playNumberDisplays.Length)
        {
            if (playNumberDisplays[indexToShow] != null)
            {
                playNumberDisplays[indexToShow].SetActive(true);
            }
        }
    }

    // --- Grab/Release Logic ---
    private void TryGrabObject(GameObject objToGrab)
    {
        // Make sure objToGrab is a grabbable object
        if (objToGrab == null || ((1 << objToGrab.layer) & grabbableLayer) == 0) return;

        Rigidbody2D objRb = objToGrab.GetComponent<Rigidbody2D>();
        if (objRb != null)
        {
            grabbedObject = objToGrab;
            // Option A: Parent the object (Simplest, but loses physics interaction while parented)
            grabbedObject.transform.SetParent(this.transform); // Make the grabbed object a child of the crane
            objRb.isKinematic = true; // Make it kinematic so it doesn't fall/react to physics while grabbed

            // Option B: Use a FixedJoint2D (More realistic dangling, keeps physics)
            // FixedJoint2D joint = grabbedObject.AddComponent<FixedJoint2D>();
            // joint.connectedBody = clawLeftCollider.GetComponent<Rigidbody2D>(); // Connect to one of your claw rigidbodies
            // joint.breakForce = grabStrength; // How much force it takes to break the joint
            // You would need to store the joint reference to destroy it later.

            // Optional: Apply visual squish (requires storing original scale somewhere)
            // originalGrabbedObjectScale = grabbedObject.transform.localScale;
            // grabbedObject.transform.localScale = new Vector3(originalGrabbedObjectScale.x * 1.1f, originalGrabbedObjectScale.y * 0.9f, 1f);
        }
    }

    private void ReleaseGrabbedObject()
    {
        if (grabbedObject != null)
        {
            // Revert Option A: Unparent and make dynamic again
            grabbedObject.transform.SetParent(null); // Detach from crane
            Rigidbody2D objRb = grabbedObject.GetComponent<Rigidbody2D>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // Let it fall/react to physics again
                objRb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); // A little push to simulate dropping
            }

            // Revert Option B: Destroy the joint if used
            // FixedJoint2D joint = grabbedObject.GetComponent<FixedJoint2D>();
            // if (joint != null) { Destroy(joint); }

            // Optional: Revert visual squish if applied
            // grabbedObject.transform.localScale = originalGrabbedObjectScale;

            grabbedObject = null; // Clear reference
        }
    }
}