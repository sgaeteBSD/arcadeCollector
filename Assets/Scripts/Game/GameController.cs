using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Dialog, Menu }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Camera worldCamera;

    GameState state;
    
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
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

    public GameState State => state;
}
