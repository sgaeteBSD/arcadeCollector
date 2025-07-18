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
    private bool leaving = false;
    public CraneState State => state;
    public CraneController craneController;

    [Header("UI")]
    public GameObject selectMenuUI; 
    public Button playButton;       

    private void Awake()
    {
        Instance = this;
        leaving = false;
}

private void Start()
    {
        Instance = this;

        
        SetCraneState(CraneState.Move);
        StartCoroutine(EnterSelectState());
        playButton.onClick.AddListener(OnStartGame);
    }

    private IEnumerator EnterSelectState()
    {
        yield return new WaitForSeconds(1f);
        SetCraneState(CraneState.Play);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && leaving == false)
        {
            SoundFXManager.Instance.StopMusic();
            FadeManager.Instance.FadeToScene("FreeRoam");
            leaving = true;
        }
        if (state == CraneState.Play)
        {
            craneController.HandleUpdate();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //game over or restart logic here
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
