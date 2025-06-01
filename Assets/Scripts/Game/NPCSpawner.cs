using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    private GameObject oldNPC; // This needs to be assigned dynamically or found

    private bool npcAlreadySpawned = false; // To prevent spawning multiple times

    // Use a flag to ensure the subscription/check only happens once per spawner instance
    private bool initialized = false;

    // OnEnable is called when the object becomes enabled and active.
    // It's called when scene loads, and when object is explicitly enabled.
    void OnEnable()
    {
        // We'll defer the main logic to Start to ensure singletons are fully ready.
        // But we can check if it needs immediate handling if it's already initialized.
        if (initialized && CollectionManager.Instance != null)
        {
            // This might be a re-enable during runtime, but the initial setup was done.
            // Re-subscribe just in case (though Start/OnDisable should handle this)
            CollectionManager.Instance.OnAllRequiredItemsCollected -= SpawnNPC; // Prevent double subscription
            CollectionManager.Instance.OnAllRequiredItemsCollected += SpawnNPC;
        }
    }

    // Start is called on the frame when a script is first enabled.
    // It's guaranteed to run after all Awakes.
    void Start()
    {
        if (!initialized) // Ensure this block runs only once per instance
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

                // Check immediately on Start if the condition is already met
                if (CollectionManager.Instance.HasAllRequiredNPCSpawnItems())
                {
                    KillNPC(); // Kill the old NPC if it exists
                    SpawnNPC(); // Spawn the new NPC immediately
                    Debug.Log("NPCSpawner: Condition met on Start, spawned NPC.");
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
        // Unsubscribe to prevent memory leaks if the CollectionManager persists
        // but this Spawner is destroyed or disabled.
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

        // Optional: If the NPC should only spawn once, you can unsubscribe here
        // If you destroy this NPCSpawner after spawning, OnDisable will handle unsubscribe.
        // CollectionManager.Instance.OnAllRequiredItemsCollected -= SpawnNPC;
    }

    private void KillNPC()
    {
        // IMPORTANT: oldNPC needs to be assigned correctly!
        // If oldNPC is just a [SerializeField] in the Inspector, and the NPC is instantiated/destroyed
        // dynamically, this reference will be lost.
        if (oldNPC != null)
        {
            Destroy(oldNPC);
            Debug.Log("Old NPC destroyed.");
            oldNPC = null; // Clear the reference after destroying
        }
        else
        {
            // You might need to find the old NPC if it's not directly assigned or persists
            GameObject existingNPC = GameObject.Find("OldNPC");
            if (existingNPC != null) Destroy(existingNPC);
            Debug.Log("No old NPC to destroy or oldNPC reference was null.");
        }
    }
}