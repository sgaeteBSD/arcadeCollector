using UnityEngine;

public class LevelMan : MonoBehaviour
{
    public static LevelMan Instance { get; private set; }

    public CraneController craneController;
    [Header("Level Management")]
    public static GameObject selectedLevelPrefab; // Now static!
    public static Vector3 levelPosition;
    public static int levelPlays;
    private GameObject currentLevelInstance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InstantiateLevel();
        craneController.maxPlays = levelPlays;
    }

    public void InstantiateLevel()
    {
        if (selectedLevelPrefab == null)
        {
            Debug.LogWarning("No level prefab selected!");
            return;
        }

        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        currentLevelInstance = Instantiate(selectedLevelPrefab, levelPosition, Quaternion.identity);
    }
}
