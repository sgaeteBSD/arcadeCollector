// Scripts/UI/PlayDisplayManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDisplayManager : MonoBehaviour
{
    public GameObject[] playNumberDisplays; // Assign 0 to maxPlays GameObjects here

    // Public method to update the display based on current plays
    public void UpdateDisplay(int currentPlays)
    {
        // First, disable all play number GameObjects
        foreach (GameObject display in playNumberDisplays)
        {
            if (display != null)
            {
                display.SetActive(false);
            }
        }

        // Calculate the correct index.
        // We now map currentPlays directly to the index.
        int indexToShow = Mathf.Clamp(currentPlays, 0, playNumberDisplays.Length - 1);

        // Enable only the GameObject corresponding to the current number of plays
        if (indexToShow >= 0 && indexToShow < playNumberDisplays.Length)
        {
            if (playNumberDisplays[indexToShow] != null)
            {
                playNumberDisplays[indexToShow].SetActive(true);
            }
        }
    }
}