using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
[System.Serializable]
public class CollectionData
{
    public List<string> collectedPrizeIDs = new List<string>();
}


public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

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

    public event Action OnAllRequiredItemsCollected;

    private CollectionData data = new CollectionData();

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
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

            CheckForAllRequiredItemsCollected();
        }
    }

    public bool HasPrize(string prizeID)
    {
        return data.collectedPrizeIDs.Contains(prizeID);
    }

    public bool HasAllRequiredNPCSpawnItems()
    {
        foreach (string requiredID in requiredNPCSpawnPrizeIDs)
        {
            if (!data.collectedPrizeIDs.Contains(requiredID))
            {
                return false;
            }
        }
        return true;
    }

    private void CheckForAllRequiredItemsCollected()
    {
        if (HasAllRequiredNPCSpawnItems())
        {
            OnAllRequiredItemsCollected?.Invoke();
            Debug.Log("All required NPC spawn items collected! Triggering event.");
        }
    }

    public void SaveCollection()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("CollectionData", json);
        PlayerPrefs.Save();
    }

    public void LoadCollection()
    {
        if (PlayerPrefs.HasKey("CollectionData"))
        {
            string json = PlayerPrefs.GetString("CollectionData");
            data = JsonUtility.FromJson<CollectionData>(json);
            CheckForAllRequiredItemsCollected();
        }
    }

    public List<string> GetCollectedPrizes()
    {
        return new List<string>(data.collectedPrizeIDs);
    }

    public void ClearCollection()
    {
        data.collectedPrizeIDs.Clear();
        SaveCollection();
        Debug.Log("Collection cleared.");
    }
}