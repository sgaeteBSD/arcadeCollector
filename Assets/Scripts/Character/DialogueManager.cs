using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] public GameObject dialogBox;
    [SerializeField] public Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
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
        dialogBox.SetActive(true);

        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        isShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        if (dialog != null && dialog.Lines != null && dialog.Lines.Count > 0)
        {
            StartCoroutine(TypeDialog(dialog.Lines[0]));
        }
        else
        {
            Debug.LogWarning("DialogueManager: Attempted to show an empty or null dialogue.", this);
            EndDialogueImmediate(); //close if dialogue is empty
        }
    }

    public void HandleUpdate()
    {
        if (isShowing && Input.GetKeyDown(KeyCode.E) && !isTyping)
        {
            ++currentLine;
            if (dialog != null && dialog.Lines != null && currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) 
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }

   
    private void EndDialogue()
    {
        currentLine = 0;
        isShowing = false;
        if (dialogBox != null) 
        {
            dialogBox.SetActive(false);
        }
        onDialogFinished?.Invoke();
        OnCloseDialog?.Invoke();
    }

    private void EndDialogueImmediate()
    {
        if (isTyping)
        {
            StopAllCoroutines(); 
            isTyping = false;
        }
        EndDialogue();
    }
}