using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Rendering; // This is an editor-only namespace, generally not needed in runtime scripts
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add this if you need SceneManager for checks

public class DialogueManager : MonoBehaviour
{
    [SerializeField] public GameObject dialogBox;
    [SerializeField] public Text dialogText; // Or TextMeshProUGUI
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        // --- PROPER SINGLETON WITH DONTDESTROYONLOAD ---
        if (Instance != null && Instance != this)
        {
            // An instance already exists, and it's not this one.
            // This means another DialogueManager (likely the persistent one) is already active.
            // Destroy this duplicate.
            Destroy(gameObject);
            return; // Stop execution for this duplicate object
        }
        else
        {
            // This is the first or the persistent instance. Set it.
            Instance = this;
            // Make sure this object (and its children) persist across scene loads.
            DontDestroyOnLoad(gameObject);
        }
        // --- END SINGLETON ---

        // Ensure initial state
        if (dialogBox != null)
        {
            dialogBox.SetActive(false); // Dialogue box should start hidden
        }
        else
        {
            Debug.LogError("DialogueManager: dialogBox is not assigned in the Inspector!", this);
        }
        if (dialogText == null)
        {
            Debug.LogError("DialogueManager: dialogText is not assigned in the Inspector!", this);
        }
    }

    Dialogue dialog;
    Action onDialogFinished;
    int currentLine = 0;
    bool isTyping;

    public bool isShowing { get; private set; }

    public IEnumerator ShowDialog(Dialogue dialog, Action onFinished = null)
    {
        // Optional: Ensure the dialogue box is active before the yield,
        // so it doesn't flash if there's a frame delay.
        dialogBox.SetActive(true);

        yield return new WaitForEndOfFrame(); // This ensures all UI updates for the current frame are done

        OnShowDialog?.Invoke();

        isShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        // StartCoroutine(TypeDialog(dialog.Lines[0]));
        // Make sure to handle the case where dialog.Lines might be empty to prevent index out of bounds
        if (dialog != null && dialog.Lines != null && dialog.Lines.Count > 0)
        {
            StartCoroutine(TypeDialog(dialog.Lines[0]));
        }
        else
        {
            Debug.LogWarning("DialogueManager: Attempted to show an empty or null dialogue.", this);
            EndDialogueImmediate(); // Close if dialogue is empty
        }
    }

    public void HandleUpdate()
    {
        // Only allow input if dialogue is showing AND not currently typing
        if (isShowing && Input.GetKeyDown(KeyCode.E) && !isTyping)
        {
            ++currentLine;
            if (dialog != null && dialog.Lines != null && currentLine < dialog.Lines.Count) // Add null checks
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public IEnumerator TypeDialog(string line) // Renamed parameter from 'dialog' to 'line' for clarity
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) // Use 'line' parameter
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }

    // New helper method to end dialogue cleanly
    private void EndDialogue()
    {
        currentLine = 0;
        isShowing = false;
        if (dialogBox != null) // Add null check
        {
            dialogBox.SetActive(false);
        }
        onDialogFinished?.Invoke();
        OnCloseDialog?.Invoke();
    }

    // For immediate ending if something goes wrong or dialogue is empty
    private void EndDialogueImmediate()
    {
        if (isTyping)
        {
            StopAllCoroutines(); // Stop any ongoing typing
            isTyping = false;
        }
        EndDialogue();
    }
}