using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;



public class PrizeDetector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessPrize(other.gameObject);
    }
    void ProcessPrize(GameObject prizeObject)
    {
        // Check if the object that entered the trigger has the "Prize" tag
        if (prizeObject.CompareTag("Prize"))
        {
            PrizeInfo prizeInfo = prizeObject.GetComponent<PrizeInfo>();

            if (prizeInfo != null)
            {
                Debug.Log($"Prize Won! Name: {prizeInfo.prizeName}");
                // Add to your collection
                Destroy(prizeObject);
            }
            else
            {
                Destroy(prizeObject);
            }
        }
        else
        {
            Destroy(prizeObject);
        }
    }

}