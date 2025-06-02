using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Dialog, Menu, Interact }

public class GameController : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;
    [SerializeField] public DialogueManager dg;
    [SerializeField] Camera worldCamera;

    GameState state;
    
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject); 

        if (dg == null) 
        {
            dg = DialogueManager.Instance; 
        }
        if (dg == null)
        {
            Debug.LogError("GameController: DialogueManager instance is null after Awake. Make sure it's present and set up correctly.", this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.FreeRoam;
        DialogueManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogueManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogueManager.Instance.HandleUpdate();
        }


    }

    public void SetGameState(GameState newState)
    {
        state = newState;
    }

    public void SetDialogMan(DialogueManager newState)
    {
        dg = newState;
    }

    public GameState State => state;
}
