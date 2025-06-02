using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private GameObject interactionPromptSpriteObject; [SerializeField] private AudioClip interacty;
    public bool leaving;

    void Awake()
    {
        leaving = false;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        if (SceneManager.GetActiveScene().name == "CraneGame")
        {
            gameObject.SetActive(false);
        }

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
        if (SceneManager.GetActiveScene().name == "CraneGame" || SceneManager.GetActiveScene().name == "StartScene") ;
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

    private void CheckForInteractablePrompt()
    {
        if (interactionPromptSpriteObject == null) return;

        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

        if (collider != null && collider.GetComponent<Interactable>() != null)
        {
            if (!interactionPromptSpriteObject.activeSelf)
            {
                interactionPromptSpriteObject.SetActive(true);
            }
        }
        else
        {
            if (interactionPromptSpriteObject.activeSelf)
            {
                interactionPromptSpriteObject.SetActive(false);
            }
        }
    }
}