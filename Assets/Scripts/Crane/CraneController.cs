// Scripts/Crane/CraneController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// No longer needs UnityEngine.UI if PlayDisplayManager handles it

public class CraneController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float leftLimit = -7f;
    public float rightLimit = 7f;

    [Header("Plays")]
    public int maxPlays = 5;
    private int currentPlays;

    [Header("References")]
    // Reference to the new ClawMechanism script (on this same GameObject)
    public ClawController clawMechanism;
    // Reference to the PlayDisplayManager script (can be on another GameObject)
    public PlayDisplayManager playDisplayManager;

    private bool isDropping = false; // Flag to prevent re-triggering drop

    private void Start()
    {
        currentPlays = maxPlays;
        // Initial display update
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }
        // Ensure claws are open at game start (delegate to ClawMechanism)
        if (clawMechanism != null)
        {
            clawMechanism.SetClawOpen();
        }
    }

    private void Update()
    {
        // Prevent horizontal movement and dropping if currently dropping or out of plays
        if (isDropping || currentPlays <= 0) return;

        // Horizontal movement
        float h = Input.GetAxisRaw("Horizontal");
        transform.position += Vector3.right * h * moveSpeed * Time.deltaTime;
        ClampPosition();

        // Drop initiation
        if (Input.GetKeyDown(KeyCode.Space) && currentPlays > 0)
        {
            StartCoroutine(PerformDropSequence());
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        transform.position = pos;
    }

    private IEnumerator PerformDropSequence()
    {
        isDropping = true;

        // Store the crane's initial Y position before dropping
        float initialCraneY = transform.position.y;

        // Pass control to the ClawMechanism to handle the entire drop sequence
        GameObject grabbedItem = null;
        if (clawMechanism != null)
        {
            // Call the ClawMechanism coroutine
            IEnumerator clawDropEnumerator = clawMechanism.DropAndGrab(initialCraneY);

            // Iterate through the ClawMechanism's coroutine
            while (clawDropEnumerator.MoveNext())
            {
                // If the current element yielded by ClawMechanism is a GameObject,
                // it's our grabbed object.
                if (clawDropEnumerator.Current is GameObject obj)
                {
                    grabbedItem = obj;
                }
                yield return clawDropEnumerator.Current; // Yield whatever ClawMechanism yields (null or WaitForSeconds)
            }
            // After the loop, 'grabbedItem' will hold the final object returned by ClawMechanism.
        }

        // After the claws have completed their cycle (returned to top and opened)
        currentPlays--;
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }

        // Handle the grabbed item here (e.g., score points, move to prize chute)
        if (grabbedItem != null)
        {
            Debug.Log($"Crane completed cycle. Grabbed: {grabbedItem.name}. Now handling prize delivery.");
            // Example: If you have a prize chute, move it there.
            // For now, ClawMechanism will handle releasing it.
            if (clawMechanism != null)
            {
                clawMechanism.ReleaseGrabbedObject(grabbedItem); // Explicitly release it here after use
            }
        }

        isDropping = false;

        if (currentPlays <= 0)
        {
            Debug.Log("Out of plays. Reset level!");
            // TODO: Add game over or restart logic here
        }
    }
}