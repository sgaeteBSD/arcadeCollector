using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class InteractObject : MonoBehaviour, Interactable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(Transform initiator)
    {
        if (GameController.Instance.State == GameState.FreeRoam)
        {
            GameController.Instance.SetGameState(GameState.Interact);
        }
    }
}
