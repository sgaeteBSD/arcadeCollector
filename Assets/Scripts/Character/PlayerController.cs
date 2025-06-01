using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        if (SceneManager.GetActiveScene().name == "CraneGame")
        {
            gameObject.SetActive(false);
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private Vector2 input;

    private Character character;


    // Start is called before the first frame update
    public void Start()
    {
        character = GetComponent<Character>();
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (SceneManager.GetActiveScene().name == "CraneGame")
            this.gameObject.SetActive(false);
        else
        {
            this.gameObject.SetActive(true);
        }
        if (!character.IsMoving)
        {
            //raw means val is always -1 or 1 (good for grid movement)
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //remove diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input/*, secondary action*/));
            }
        }
        character.HandleUpdate();
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }


    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
}