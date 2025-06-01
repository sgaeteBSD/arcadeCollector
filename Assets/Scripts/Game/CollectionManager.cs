using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections; // Needed for Action

[System.Serializable]
public class CollectionData
{
    public List<string> collectedPrizeIDs = new List<string>();
}


public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    // --- New setup for tracking specific items ---
    [Tooltip("The specific item IDs required to trigger the NPC spawn.")]
    [SerializeField]
    private List<string> requiredNPCSpawnPrizeIDs = new List<string>
    {
        "Miko",
        "Laplus",
        "Korone",
        "Kanade",
        "Baelz"
    };

    // An event that other scripts (like your NPC spawner) can subscribe to
    public event Action OnAllRequiredItemsCollected;
    // --- End new setup ---

    private CollectionData data = new CollectionData();

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
        // This allows other scripts to start a coroutine that will be managed
        // by the persistent CollectionManager, ensuring it runs.
        return StartCoroutine(coroutine);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCollection();
    }

    public void AddPrize(string prizeID)
    {
        if (!data.collectedPrizeIDs.Contains(prizeID))
        {
            data.collectedPrizeIDs.Add(prizeID);
            SaveCollection();

            // --- New check after adding a prize ---
            CheckForAllRequiredItemsCollected();
            // --- End new check ---
        }
    }

    public bool HasPrize(string prizeID)
    {
        return data.collectedPrizeIDs.Contains(prizeID);
    }

    // --- New method to check if all required items are collected ---
    public bool HasAllRequiredNPCSpawnItems()
    {
        foreach (string requiredID in requiredNPCSpawnPrizeIDs)
        {
            if (!data.collectedPrizeIDs.Contains(requiredID))
            {
                return false; // Found a required item that's not collected
            }
        }
        return true; // All required items are collected
    }

    // --- New method to trigger the event ---
    private void CheckForAllRequiredItemsCollected()
    {
        if (HasAllRequiredNPCSpawnItems())
        {
            // Only invoke if there are listeners (subscribers)
            OnAllRequiredItemsCollected?.Invoke();
            Debug.Log("All required NPC spawn items collected! Triggering event.");
            // You might want to unsubscribe or set a flag here if this event should only fire once
            // For example: OnAllRequiredItemsCollected = null;
        }
    }
    // --- End new method ---

    public void SaveCollection()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("CollectionData", json);
        PlayerPrefs.Save(); // Ensure data is written to disk immediately
    }

    public void LoadCollection()
    {
        if (PlayerPrefs.HasKey("CollectionData"))
        {
            string json = PlayerPrefs.GetString("CollectionData");
            data = JsonUtility.FromJson<CollectionData>(json);
            // After loading, check if the condition is already met from a previous session
            CheckForAllRequiredItemsCollected();
        }
    }

    public List<string> GetCollectedPrizes()
    {
        return new List<string>(data.collectedPrizeIDs);
    }

    // Optional: For testing purposes, to reset collected items
    public void ClearCollection()
    {
        data.collectedPrizeIDs.Clear();
        SaveCollection();
        Debug.Log("Collection cleared.");
    }
}