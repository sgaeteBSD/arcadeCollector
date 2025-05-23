using System.Collections;
using UnityEngine;

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
    public ClawController clawController;
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
        // Ensure claws are open at game start (delegate to ClawController)
        if (clawController != null)
        {
            clawController.SetClawOpen();
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

        float initialCraneY = transform.position.y; // Crane's initial Y position
        IEnumerator clawCycle = clawController.DropAndGrab(initialCraneY, rightLimit, moveSpeed);

        GameObject grabbedItem = null;
        while (clawCycle.MoveNext())
        {
            if (clawCycle.Current is GameObject obj)
            {
                grabbedItem = obj;
            }
            yield return clawCycle.Current;
        }
        Vector3 targetDropPosition = new Vector3(rightLimit, transform.position.y, transform.position.z);

        // --- NEW: Prize Delivery Phase ---
        if (grabbedItem != null) // Only perform if an item was grabbed
        {
            Debug.Log($"Grabbed {grabbedItem.name}. Moving to prize drop zone.");

            // Move to the right end slowly
            yield return new WaitForSeconds(0.3f); // Short pause to see
            clawController.ReleaseGrabbedObject(grabbedItem); // Release the item
            while (Vector3.Distance(transform.position, targetDropPosition) > 0.1f) // Move until very close
            {
                transform.position = Vector3.MoveTowards(transform.position, targetDropPosition, moveSpeed * Time.deltaTime);
                ClampPosition(); // Ensure it stays within bounds during move
                yield return null; // Wait for next frame
            }
            transform.position = targetDropPosition; // Snap to exact target
            Debug.Log($"Reached drop zone. Releasing {grabbedItem.name}.");
            if (clawController != null)
            {
                yield return new WaitForSeconds(0.4f); // Short pause to see the drop
                clawController.SetClawOpen();
            }
            yield return new WaitForSeconds(0.5f); // Short pause to see the drop
        }
        else
        {
            clawController.SetClawOpen();
            yield return new WaitForSeconds(0.3f); // Short pause to see
            while (Vector3.Distance(transform.position, targetDropPosition) > 0.1f) // Move until very close
            {
                transform.position = Vector3.MoveTowards(transform.position, targetDropPosition, moveSpeed * Time.deltaTime);
                ClampPosition(); // Ensure it stays within bounds during move
                yield return null; // Wait for next frame
            }
            transform.position = targetDropPosition; // Snap to exact target
        }
        // --- END NEW PRIZE DELIVERY PHASE ---

        // After the claws have completed their cycle (returned to top and opened)
        currentPlays--;
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }

        isDropping = false;

        if (currentPlays <= 0)
        {
            Debug.Log("Out of plays. Reset level!");
            // TODO: Add game over or restart logic here
        }
    }
}