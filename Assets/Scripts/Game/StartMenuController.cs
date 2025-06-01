using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    private bool fading = false;
    private string dname = "FreeRoam";
    public void OnStartClick()
    {
        fading = true;
        FadeManager.Instance.FadeToScene(dname);
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) ||  Input.GetKeyUp(KeyCode.Space)) && fading != true)
        {
            fading = true;
            FadeManager.Instance.FadeToScene(dname);
        }
    }
}
