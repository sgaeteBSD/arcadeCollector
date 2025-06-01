using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    private bool fading = false;
    private string dname = "FreeRoam";
    [SerializeField] private AudioClip start;
    public void OnStartClick()
    {
        SoundFXManager.Instance.PlaySFXClip(start, this.transform, 0.5f);
        fading = true;
        FadeManager.Instance.FadeToScene(dname);
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) ||  Input.GetKeyUp(KeyCode.Space)) && fading != true)
        {
            SoundFXManager.Instance.PlaySFXClip(start, this.transform, 0.5f);
            fading = true;
            FadeManager.Instance.FadeToScene(dname);
        }
    }
}
