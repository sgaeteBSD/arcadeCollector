using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectionData
{
    public List<string> collectedPrizeIDs = new List<string>();
}

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    private CollectionData data = new CollectionData();

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
        }
    }

    public bool HasPrize(string prizeID)
    {
        return data.collectedPrizeIDs.Contains(prizeID);
    }

    public void SaveCollection()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("CollectionData", json);
    }

    public void LoadCollection()
    {
        if (PlayerPrefs.HasKey("CollectionData"))
        {
            string json = PlayerPrefs.GetString("CollectionData");
            data = JsonUtility.FromJson<CollectionData>(json);
        }
    }

    public List<string> GetCollectedPrizes()
    {
        return new List<string>(data.collectedPrizeIDs);
    }
}
