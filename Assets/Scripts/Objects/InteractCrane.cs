using UnityEngine;

public class InteractCrane : MonoBehaviour, Interactable
{
    public string craneSceneName = "CraneGame"; // assign in Inspector or hardcode
    [SerializeField] public GameObject levelPrefab;
    public int plays = 0;

    public void Interact(Transform initiator)
    {
        if (GameController.Instance.State == GameState.FreeRoam)
        {
            LevelMan.selectedLevelPrefab = levelPrefab;
            LevelMan.levelPlays = plays;
            // Begin loading the crane game scene
            FadeManager.Instance.FadeToScene(craneSceneName);
        }
    }
}
