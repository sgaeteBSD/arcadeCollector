using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Add this namespace for UnityEvent

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    // --- NEW: Event to be triggered after THIS NPC's dialogue finishes ---
    [Tooltip("Actions to perform after this NPC's dialogue is finished.")]
    public UnityEvent OnDialogueFinishedActions; // Assign functions in Inspector!
    // --- END NEW ---

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        // Only allow interaction if not already in dialogue
        if (state == NPCState.Idle && !DialogueManager.Instance.isShowing)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            // Pass a lambda function (anonymous method) as the onFinished callback
            StartCoroutine(DialogueManager.Instance.ShowDialog(dialog, () =>
            {
                // This code runs AFTER the dialogue is fully closed in DialogueManager
                idleTimer = 0f;
                state = NPCState.Idle; // Return NPC to Idle state

                // --- NEW: Invoke the specific event for this NPC ---
                OnDialogueFinishedActions?.Invoke(); // Fire the event!
                Debug.Log($"NPC {gameObject.name}: Dialogue finished. Invoking OnDialogueFinishedActions.");
                // --- END NEW ---

                // Optional: If you also need to tell GameController to go back to FreeRoam,
                // do it here or ensure DialogueManager's OnCloseDialog event is handled by GameController.
                // If GameController manages the global state:
                // GameController.Instance.State = GameState.FreeRoam;
            }));
        }
        else if (DialogueManager.Instance.isShowing)
        {
            // If dialogue is already showing (e.g., player spams E),
            // tell the DialogueManager to advance the current dialogue line.
            // This assumes DialogueManager.HandleUpdate() is only called when GameController.State is Dialogue.
            // If GameController handles state for dialogue advancing, then this might not be needed directly here.
            // However, your DialogueManager.HandleUpdate() is public, so if it's called by GameController,
            // this "else if" block means the NPC wouldn't advance it itself.
            // Generally, only the GameController or DialogueManager's internal Update should advance dialogue based on state.
            // So, this block is mostly for preventing starting a *new* dialogue while one is active.
            Debug.Log("Dialogue already showing, cannot start new one.");
        }
    }

    private void Update()
    {
        // Important: Ensure GameController's state handling is correct here.
        // If GameController's Update calls PlayerController.HandleUpdate and also DialogueManager.HandleUpdate
        // based on GameState, then this NPC's Update might not need to do this check directly.
        // However, if NPCController itself needs to check for movement in FreeRoam:
        if (GameController.Instance.State == GameState.FreeRoam)
        {
            if (state == NPCState.Idle)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer > timeBetweenPattern)
                {
                    idleTimer = 0f;
                    if (movementPattern.Count > 0)
                    {
                        StartCoroutine(Walk());
                    }
                }
            }
            // Only allow character.HandleUpdate if NPC is not in dialogue state.
            // This is assuming character.HandleUpdate contains the animation logic and potential movement
            // that doesn't need to be paused during dialogue. If it contains player input logic,
            // then PlayerController should handle that based on GameState.
            if (state != NPCState.Dialog)
            {
                character.HandleUpdate();
            }
        }
        // If GameController.Instance.State is Dialogue, then NPC's movement and character.HandleUpdate should be paused.
        // This is controlled by the outer if (GameController.Instance.State != GameState.FreeRoam) return;
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        // Ensure character.Move waits until movement is complete
        yield return character.Move(movementPattern[currentPattern]);

        // Only advance pattern if the character actually moved (e.g., not blocked)
        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog }