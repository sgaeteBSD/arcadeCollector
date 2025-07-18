using System.Collections;
using Unity.VisualScripting;
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
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }
        // ClawController's Awake already handles setting claws open
    }

    public void HandleUpdate()
    {
        if (isDropping || currentPlays <= 0) return;

        float h = Input.GetAxisRaw("Horizontal");
        transform.position += Vector3.right * h * moveSpeed * Time.deltaTime;
        ClampPosition();

        if (Input.GetKeyDown(KeyCode.Space) && currentPlays > 0)
        {
            StartCoroutine(PerformDropSequence());
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            // TODO: Add game over or restart logic here
            LevelMan.Instance.InstantiateLevel();
            ResetCrane();
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
        currentPlays--;
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }
        isDropping = true;

        float initialCraneY = transform.position.y;

        // This will now handle the entire drop, grab, ascent, and move-to-drop-zone sequence
        IEnumerator clawCycle = clawController.DropAndGrab(initialCraneY, rightLimit, moveSpeed);

        GameObject grabbedItem = null;
        while (clawCycle.MoveNext())
        {
            // Check if the enumerator yielded a GameObject (the grabbed item)
            if (clawCycle.Current is GameObject obj)
            {
                grabbedItem = obj;
            }
            yield return clawCycle.Current; // Continue the ClawController's coroutine
        }

        isDropping = false;

        if ((currentPlays <= 0) && (!grabbedItem || !grabbedItem.CompareTag("Prize")))
        {
            Debug.Log("Out of plays. Reset level!");
        }
    }
    public void ResetCrane()
    {
        StopCoroutine(PerformDropSequence());
        transform.position = new Vector3(rightLimit, 3.5f, 0f);
        LevelMan.Instance.InstantiateLevel();
        currentPlays = maxPlays;
        if (playDisplayManager != null)
        {
            playDisplayManager.UpdateDisplay(currentPlays);
        }
    }
}