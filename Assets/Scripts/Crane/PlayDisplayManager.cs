using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI; 

public class PlayDisplayManager : MonoBehaviour
{
    public GameObject[] playNumberDisplays; //

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