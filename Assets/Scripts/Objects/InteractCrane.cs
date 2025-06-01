using UnityEngine;
using UnityEngine.UIElements;

public class InteractCrane : MonoBehaviour, Interactable
{
    public string craneSceneName = "CraneGame"; // assign in Inspector or hardcode
    [SerializeField] public GameObject levelPrefab;
    [SerializeField] public Vector3 levelPosition;
    [SerializeField] public float leftLimit = -7f;
    [SerializeField] public float rayDist = 0.6f;
    [SerializeField] public float rayWidth = 2.5f;
    private bool leaving = false;
    public int plays = 0;

    public void Interact(Transform initiator)
    {
        if (GameController.Instance.State == GameState.FreeRoam && leaving == false)
        {
            leaving = true;
            SoundFXManager.Instance.StopMusic();
            LevelMan.selectedLevelPrefab = levelPrefab;
            LevelMan.levelPosition = levelPosition;
            LevelMan.rayWidth = rayWidth;
            LevelMan.leftLimit = leftLimit;
            LevelMan.rayDist = rayDist;
            LevelMan.levelPlays = plays;
            // Begin loading the crane game scene
            FadeManager.Instance.FadeToScene(craneSceneName);
        }
    }
}
