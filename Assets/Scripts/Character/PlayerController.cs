using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // Changed from Image to GameObject for the sprite prompt
    [SerializeField] private GameObject interactionPromptSpriteObject; // Assign this in the Inspector!
    [SerializeField] private AudioClip interacty;
    public bool leaving;

    void Awake()
    {
        leaving = false;
        // --- Your existing singleton setup (ensure DontDestroyOnLoad is applied if player persists) ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            // Example of DontDestroyOnLoad if your PlayerController is meant to persist
            // DontDestroyOnLoad(gameObject);
        }

        if (SceneManager.GetActiveScene().name == "CraneGame")
        {
            gameObject.SetActive(false);
        }
        // --- End of existing singleton setup ---

        // Ensure the interaction prompt sprite object is initially hidden
        if (interactionPromptSpriteObject != null)
        {
            interactionPromptSpriteObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("InteractionPromptSpriteObject not assigned in PlayerController. Please assign the GameObject with the SpriteRenderer in the Inspector.", this);
        }
    }

    private Vector2 input;
    private Character character;

    public void Start()
    {
        character = GetComponent<Character>();
        leaving = false;
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && leaving == false && SceneManager.GetActiveScene().name == "FreeRoam")
        {
            leaving = true;
            SoundFXManager.Instance.StopMusic();
            FadeManager.Instance.FadeToScene("StartScene");
        }
        if (SceneManager.GetActiveScene().name == "CraneGame" || SceneManager.GetActiveScene().name == "StartScene");
        //SoundFXManager.Instance.StopMusic();
        else
        {
            this.gameObject.SetActive(true);
        }

        if (!character.IsMoving && SceneManager.GetActiveScene().name == "FreeRoam")
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input));
            }
        }
        character.HandleUpdate();

        // --- NEW LOGIC FOR SHOWING/HIDING INTERACTION PROMPT SPRITE ---
        CheckForInteractablePrompt();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }


    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
            SoundFXManager.Instance.PlaySFXClip(interacty, transform, 0.8f);
        }
    }

    // New method to manage the interaction prompt sprite's visibility
    private void CheckForInteractablePrompt()
    {
        // Only proceed if the prompt sprite object is assigned
        if (interactionPromptSpriteObject == null) return;

        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

        // If an interactable object is detected
        if (collider != null && collider.GetComponent<Interactable>() != null)
        {
            // Show the prompt sprite if it's not already active
            if (!interactionPromptSpriteObject.activeSelf)
            {
                interactionPromptSpriteObject.SetActive(true);
            }
        }
        else // No interactable object detected
        {
            // Hide the prompt sprite if it's active
            if (interactionPromptSpriteObject.activeSelf)
            {
                interactionPromptSpriteObject.SetActive(false);
            }
        }
    }
}