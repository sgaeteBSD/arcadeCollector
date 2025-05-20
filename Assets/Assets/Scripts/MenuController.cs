using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject modelRoot;
    void Start()
    {
        menuCanvas.SetActive(false);
        modelRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool isOpening = !menuCanvas.activeSelf;
            menuCanvas.SetActive(isOpening);
            modelRoot.SetActive(isOpening);

            if (isOpening)
            {
                GameController.Instance.SetGameState(GameState.Menu);
            }
            else
            {
                GameController.Instance.SetGameState(GameState.FreeRoam);
            }
        }
    }
}
