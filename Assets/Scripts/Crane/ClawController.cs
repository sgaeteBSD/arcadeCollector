using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    [Header("Claw Parts")]
    public Transform clawLeft;
    public Transform clawRight;

    [Header("Claw Joint Settings")]
    public Transform clawPivotPoint;
    public float clawOpenAngle = 95f;
    public float clawCloseAngle = -7f;
    public float clawOpenCloseSpeed = 100f;

    [Header("Claw Movement (Crane Body)")]
    public float verticalMoveSpeed = 5f; // Vertical speed (descent/ascent)
    public float maxDropDistance = 5f; // Max distance claws can drop from crane body

    [Header("Collision Detection")] // New Header for collision settings
    public float pivotRaycastDistance = 0.2f; // How far down to check for obstacles from the pivot point
    public LayerMask obstacleLayer; // Layer for objects that should stop the crane descent

    [Header("Grab Logic")]
    public float grabForceMagnitude = 1000f; // Force applied when trying to grab
    public float jointBreakForce = 100f; // Force at which the DistanceJoint2D breaks
    public float jointBreakTorque = 100f; // Torque at which the DistanceJoint2D breaks

    public LayerMask grabbableLayer;

    // Private references for physics components
    private Rigidbody2D rbLeftClaw;
    private Rigidbody2D rbRightClaw;
    private HingeJoint2D hingeLeftClaw;
    private HingeJoint2D hingeRightClaw;
    private bool isClawClosing = false; // To manage closing state
    private bool isClawOpening = false; // To manage opening state

    private void Awake()
    {
        // Get Rigidbody2D and HingeJoint2D references once
        rbLeftClaw = clawLeft.GetComponent<Rigidbody2D>();
        rbRightClaw = clawRight.GetComponent<Rigidbody2D>();
        hingeLeftClaw = clawLeft.GetComponent<HingeJoint2D>();
        hingeRightClaw = clawRight.GetComponent<HingeJoint2D>();

        if (rbLeftClaw == null || rbRightClaw == null || hingeLeftClaw == null || hingeRightClaw == null)
        {
            Debug.LogError("ClawController: Missing Rigidbody2D or HingeJoint2D on claw parts! Disabling script.");
            enabled = false;
            return;
        }

        if (clawPivotPoint == null)
        {
            Debug.LogError("ClawController: 'Claw Pivot Point' is not assigned! Disabling script.");
            enabled = false;
            return;
        }
        // Ensure claws start open
        //SetClawOpen();
    }

    private void SetupHingeJoint(HingeJoint2D hinge, Rigidbody2D rb, Transform pivot)
    {
        hinge.autoConfigureConnectedAnchor = false;
        hinge.connectedBody = pivot.GetComponent<Rigidbody2D>();
        hinge.useLimits = true;
        hinge.enableCollision = true;
        hinge.motor = new JointMotor2D { motorSpeed = 0, maxMotorTorque = grabForceMagnitude };
        hinge.useMotor = false;
    }

    // FixedUpdate is where physics calculations should happen
    private void FixedUpdate()
    {
        if (isClawClosing)
        {
            float targetAngle = clawCloseAngle * 1.3f;
            float currentAngleLeft = hingeLeftClaw.jointAngle;
            float currentAngleRight = hingeRightClaw.jointAngle;

            // Apply motor force to close claws if not already at limit
            if (Mathf.Abs(currentAngleLeft - targetAngle) > 0.1f)
            {
                hingeLeftClaw.motor = new JointMotor2D { motorSpeed = -clawOpenCloseSpeed, maxMotorTorque = grabForceMagnitude };
                hingeLeftClaw.useMotor = true;
            }
            else
            {
                hingeLeftClaw.useMotor = false; // Stop motor when at target
            }

            // Note the negative for right claw's motorSpeed and target angle comparison
            if (Mathf.Abs(currentAngleRight - (-targetAngle)) > 0.1f)
            {
                hingeRightClaw.motor = new JointMotor2D { motorSpeed = clawOpenCloseSpeed, maxMotorTorque = grabForceMagnitude };
                hingeRightClaw.useMotor = true;
            }
            else
            {
                hingeRightClaw.useMotor = false; // Stop motor when at target
            }
        }
        else if (isClawOpening)
        {
            float targetAngle = clawOpenAngle;
            float currentAngleLeft = hingeLeftClaw.jointAngle;
            float currentAngleRight = hingeRightClaw.jointAngle;

            // Apply motor force to open claws
            if (Mathf.Abs(currentAngleLeft - targetAngle) > 0.1f)
            {
                hingeLeftClaw.motor = new JointMotor2D { motorSpeed = clawOpenCloseSpeed, maxMotorTorque = grabForceMagnitude };
                hingeLeftClaw.useMotor = true;
            }
            else
            {
                hingeLeftClaw.useMotor = false;
            }

            if (Mathf.Abs(currentAngleRight - (-targetAngle)) > 0.1f)
            {
                hingeRightClaw.motor = new JointMotor2D { motorSpeed = -clawOpenCloseSpeed, maxMotorTorque = grabForceMagnitude };
                hingeRightClaw.useMotor = true;
            }
            else
            {
                hingeRightClaw.useMotor = false;
            }
        }
    }

    // Set the claw open/close state. Called from coroutines.
    public void SetClawOpen()
    {
        isClawClosing = false;
        isClawOpening = true;
    }

    public void SetClawClosed()
    {
        isClawOpening = false;
        isClawClosing = true;
    }

    // Coroutine to perform the entire drop, grab, and ascent cycle
    public IEnumerator DropAndGrab(float craneInitialY, float targetDropX, float craneMoveSpeed)
    {
        float targetY = craneInitialY - maxDropDistance;
        SetClawOpen();
        yield return new WaitForSeconds(0.1f); // Short delay to allow open state to apply

        // --- Phase 1: Descent ---
        while (transform.position.y > targetY)
        {
            // Raycast downwards from the pivot point to detect obstacles
            RaycastHit2D hit = Physics2D.Raycast(clawPivotPoint.position, Vector2.down, pivotRaycastDistance, obstacleLayer);

            // For debugging purposes, draw the ray in the editor
            Debug.DrawRay(clawPivotPoint.position, Vector2.down * pivotRaycastDistance, Color.red);

            if (hit.collider != null)
            {
                Debug.Log($"Claw pivot hit: {hit.collider.name}. Stopping descent.");
                break; // Stop descent if an obstacle is hit
            }

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), verticalMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap to the stopped position if the loop broke due to hitting an object,
        // otherwise snap to the original targetY if maxDropDistance was reached.
        // This ensures the crane doesn't "overshoot" when it hits something.
        // For simplicity, we'll just keep the current position if it stopped due to a hit.
        // If you want to snap to the hit point, you'd calculate that based on hit.point.y
        // For now, it simply stops at the position it was in when the raycast hit.

        yield return new WaitForSeconds(0.2f);

        // --- Phase 2: Close Claws and Attempt Grab ---
        SetClawClosed();
        float grabAttemptDuration = 1.0f; // How long to try and grab for
        float timer = 0f;
        while (timer < grabAttemptDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);

        // --- Phase 3: Ascent with or without grabbed object ---
        while (transform.position.y < craneInitialY)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, craneInitialY, transform.position.z), verticalMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, craneInitialY, transform.position.z);

        Vector3 targetDropPosition = new Vector3(targetDropX, transform.position.y, transform.position.z);

        // --- Phase 4: Horizontal Movement to Drop Point ---
        while (Vector3.Distance(transform.position, targetDropPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDropPosition, craneMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetDropPosition;
        yield return new WaitForSeconds(0.5f);

        // --- Phase 5: Release Object and Open Claws ---
        yield return new WaitForSeconds(0.2f);
        SetClawOpen();
        yield return new WaitForSeconds(0.5f);
    }
}