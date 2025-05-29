using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeObj : MonoBehaviour
{
    [Tooltip("Unique ID for this prize. This ID MUST match an entry in your PrizeDatabase ScriptableObject.")]
    public string prizeID;

    void Start()
    {
        // Basic validation
        if (string.IsNullOrEmpty(prizeID))
        {
            Debug.LogError($"Prize '{gameObject.name}' is missing a prizeID! This is required to show the correct 3D model in the UI.", this);
        }
        if (!CompareTag("Prize")) // Assuming you still use the "Prize" tag for detection
        {
            Debug.LogWarning($"Prize '{gameObject.name}' is not tagged as 'Prize'. The PrizeDetector might not pick it up.", this);
        }
    }
}
