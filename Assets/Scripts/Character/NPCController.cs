using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialogue dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    [Tooltip("Actions to perform after this NPC's dialogue is finished.")]
    public UnityEvent OnDialogueFinishedActions;

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
        if (state == NPCState.Idle && !DialogueManager.Instance.isShowing)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogueManager.Instance.ShowDialog(dialog, () =>
{
    idleTimer = 0f;
    state = NPCState.Idle;
    OnDialogueFinishedActions?.Invoke(); Debug.Log($"NPC {gameObject.name}: Dialogue finished. Invoking OnDialogueFinishedActions.");

}));
        }
        else if (DialogueManager.Instance.isShowing)
        {
            Debug.Log("Dialogue already showing, cannot start new one.");
        }
    }

    private void Update()
    {
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
            if (state != NPCState.Dialog)
            {
                character.HandleUpdate();
            }
        }
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog }