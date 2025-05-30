using UnityEngine;
using UnityEngine.UIElements;

public class InteractCrane : MonoBehaviour, Interactable
{
    public string craneSceneName = "CraneGame"; // assign in Inspector or hardcode
    [SerializeField] public GameObject levelPrefab;
    [SerializeField] public Vector3 levelPosition;
    public int plays = 0;

    public void Interact(Transform initiator)
    {
        if (GameController.Instance.State == GameState.FreeRoam)
        {
            LevelMan.selectedLevelPrefab = levelPrefab;
            LevelMan.levelPosition = levelPosition;
            LevelMan.levelPlays = plays;
            // Begin loading the crane game scene
            FadeManager.Instance.FadeToScene(craneSceneName);
        }
    }
}
