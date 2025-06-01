using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum CraneState { Select, Play, Move }
public class ClawGameCon : MonoBehaviour
{
    public static ClawGameCon Instance { get; private set; }

    [Header("Game State")]
    private CraneState state;
    public CraneState State => state;
    public CraneController craneController;

    [Header("UI")]
    public GameObject selectMenuUI; // Assign in inspector (the panel)
    public Button playButton;       // Assign in inspector (the "Start" button)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Instance = this;

        // Start in "Move" transition state
        SetCraneState(CraneState.Move);
        StartCoroutine(EnterSelectState());
        playButton.onClick.AddListener(OnStartGame);
    }

    private IEnumerator EnterSelectState()
    {
        yield return new WaitForSeconds(1f); // short delay for transition or animation
        SetCraneState(CraneState.Play);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundFXManager.Instance.StopMusic();
            FadeManager.Instance.FadeToScene("FreeRoam");
        }
        if (state == CraneState.Play)
        {
            craneController.HandleUpdate();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            // TODO: Add game over or restart logic here
            LevelMan.Instance.InstantiateLevel();
            craneController.ResetCrane();
        }
    }

    public void SetCraneState(CraneState newState)
    {
        state = newState;

    }

    private void OnStartGame()
    {
        SetCraneState(CraneState.Play);
    }
}
