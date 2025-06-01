using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class AddPlayer : MonoBehaviour
{
    [SerializeField] public GameObject playerPrefab; // Renamed for clarity

    private CinemachineVirtualCamera vcam;

    void Start()
    {
        GameObject playerInstance = null;

        // 1. Check if a PlayerController instance already exists (from a previous scene load)
        if (PlayerController.Instance != null)
        {
            playerInstance = PlayerController.Instance.gameObject;
            Debug.Log("Player already exists. Not instantiating a new one.");
        }
        else
        {
            // 2. If no PlayerController instance exists, instantiate the player
            playerInstance = Instantiate(playerPrefab);
            Debug.Log("Instantiating new player.");
        }

        // 3. Set the Cinemachine camera to follow the (existing or new) player
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null && playerInstance != null)
        {
            vcam.Follow = playerInstance.transform;
        }
        else
        {
            Debug.LogWarning("CinemachineVirtualCamera or PlayerInstance not found. Camera will not follow player.");
        }

        // 4. Assign the player controller to GameController (assuming it needs it)
        if (GameController.Instance != null && playerInstance != null)
        {
            PlayerController pc = playerInstance.GetComponent<PlayerController>();
            if (pc != null)
            {
                GameController.Instance.playerController = pc;
            }
            else
            {
                Debug.LogError("Player prefab is missing PlayerController component!");
            }
        }
        else
        {
            Debug.LogWarning("GameController.Instance or PlayerInstance not found. Cannot assign player controller.");
        }

        // Optional: If your player always starts at a specific position in this scene
        // you might want to move the existing player to that position.
        // For example, if AddPlayer is on an empty GameObject at the desired spawn point:
        // if (playerInstance != null)
        // {
        //     playerInstance.transform.position = transform.position;
        //     playerInstance.transform.rotation = transform.rotation;
        // }
    }
}