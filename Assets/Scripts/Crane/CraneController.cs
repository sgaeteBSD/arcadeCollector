using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraneController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float leftLimit = -7f;
    public float rightLimit = 7f;

    [Header("Plays")]
    public int maxPlays = 5;
    private int currentPlays;
    // New: Array of GameObjects to represent the play counts, including 0
    // Make sure to set the size in the Inspector to maxPlays + 1 (e.g., 6 for maxPlays=5)
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
        float targetY = transform.position.y - clawDropDistance;
        Vector3 startPos = transform.position;

        SetClawRotation(clawOpenAngle);
        yield return null;

        while (transform.position.y > targetY)
        {
            transform.position += Vector3.down * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders();

            if (CheckClawContact())
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, grabbableLayer);
                if (hit.collider != null)
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                }
                UpdateClawColliders();
                Debug.Log("Claw hit something during descent! Stopping.");
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        float closeSpeed = 100f;
        float currentAngle = clawOpenAngle;

        while (currentAngle < clawCloseAngle)
        {
            currentAngle += closeSpeed * Time.deltaTime;
            currentAngle = Mathf.Min(currentAngle, clawCloseAngle);
            SetClawRotation(currentAngle);

            if (CheckClawContact())
                break;

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        while (transform.position.y < startPos.y)
        {
            transform.position += Vector3.up * clawMoveSpeed * Time.deltaTime;
            UpdateClawColliders();
            yield return null;
        }

        currentAngle = clawCloseAngle;

        while (currentAngle > clawOpenAngle)
        {
            currentAngle -= clawOpenSpeed * Time.deltaTime;
            currentAngle = Mathf.Max(currentAngle, clawOpenAngle);
            SetClawRotation(currentAngle);
            yield return null;
        }

        SetClawRotation(clawOpenAngle);

        transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);

        // Now remove a play
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
        if (clawLeftCollider != null)
        {
            Rigidbody2D rb = clawLeftCollider.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.MovePosition(clawLeft.position);
        }

        if (clawRightCollider != null)
        {
            Rigidbody2D rb = clawRightCollider.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.MovePosition(clawRight.position);
        }
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
        // If playNumberDisplays[0] is for 0 plays, playNumberDisplays[1] for 1 play, etc.
        // We now map currentPlays directly to the index.
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
}