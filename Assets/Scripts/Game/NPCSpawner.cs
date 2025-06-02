using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    private GameObject oldNPC;
    private bool npcAlreadySpawned = false;
    private bool initialized = false;

    void OnEnable()
    {
        if (initialized && CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnAllRequiredItemsCollected -= SpawnNPC; CollectionManager.Instance.OnAllRequiredItemsCollected += SpawnNPC;
        }
    }

    void Start()
    {
        if (!initialized)
        {
            oldNPC = GameObject.FindGameObjectWithTag("OldNPC");
            if (oldNPC == null)
            {
                Debug.LogWarning("NPCSpawner: Could not find GameObject with tag 'OldNPC'. Make sure it exists and is tagged correctly.");
            }
            else
            {
                Debug.Log("NPCSpawner: Found oldNPC dynamically by tag: " + oldNPC.name);
            }
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnAllRequiredItemsCollected += SpawnNPC;
                Debug.Log("NPCSpawner subscribed to CollectionManager event.");

                if (CollectionManager.Instance.HasAllRequiredNPCSpawnItems())
                {
                    KillNPC(); SpawnNPC(); Debug.Log("NPCSpawner: Condition met on Start, spawned NPC.");
                }
            }
            else
            {
                Debug.LogWarning("NPCSpawner: CollectionManager.Instance not found during Start. Make sure CollectionManager is in the scene and persists.", this);
            }
            initialized = true;
        }
    }


    void OnDisable()
    {
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnAllRequiredItemsCollected -= SpawnNPC;
            Debug.Log("NPCSpawner unsubscribed from CollectionManager event.");
        }
    }

    private void SpawnNPC()
    {
        if (npcAlreadySpawned)
        {
            Debug.Log("NPC already spawned. Skipping.");
            return;
        }

        if (npcPrefab == null)
        {
            Debug.LogError("NPCSpawner: NPC Prefab is not assigned!", this);
            return;
        }

        Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        npcAlreadySpawned = true;
        Debug.Log("NPC spawned at " + spawnPosition);

    }

    private void KillNPC()
    {
        if (oldNPC != null)
        {
            Destroy(oldNPC);
            Debug.Log("Old NPC destroyed.");
            oldNPC = null;
        }
        else
        {
            GameObject existingNPC = GameObject.Find("OldNPC");
            if (existingNPC != null) Destroy(existingNPC);
            Debug.Log("No old NPC to destroy or oldNPC reference was null.");
        }
    }
}