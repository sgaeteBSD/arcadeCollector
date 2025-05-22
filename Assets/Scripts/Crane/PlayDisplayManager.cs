// Scripts/UI/PlayDisplayManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine; // Still needed for MonoBehaviour and GameObject
using UnityEngine.UI; // Still needed if your playNumberDisplays are UI elements

public class PlayDisplayManager : MonoBehaviour
{
    public GameObject[] playNumberDisplays; // Assign 0 to maxPlays GameObjects here

    public void UpdateDisplay(int currentPlays)
    {
        foreach (GameObject display in playNumberDisplays)
        {
            if (display != null)
            {
                display.SetActive(false);
            }
        }

        int indexToShow = Mathf.Clamp(currentPlays, 0, playNumberDisplays.Length - 1);

        if (indexToShow >= 0 && indexToShow < playNumberDisplays.Length)
        {
            if (playNumberDisplays[indexToShow] != null)
            {
                playNumberDisplays[indexToShow].SetActive(true);
            }
        }
    }
}