using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // For PrizeInfo if it's in the same file

// You should have a PrizeInfo class defined somewhere, for example:
// [Serializable]
// public class PrizeInfo
// {
//     public string prizeID;
//     public GameObject model; // The 3D model prefab for the prize
//     public bool ud; // Unsure what 'ud' means, assuming a boolean flag
//     // Add any other prize-related data here
// }


public class TriggerProto : MonoBehaviour
{
    public GameObject popCan;
    public GameObject popBurst;
    public GameObject popText;
    public GameObject modelRoot;
    private GameObject currentModel;

    // --- Specific Prize Info field for this TriggerProto instance ---
    [Header("Prize Info to Trigger")]
    [Tooltip("Assign the specific PrizeInfo (ID, Model, etc.) that this popup will display.")]
    [SerializeField] private PrizeInfo prizeToPopup; // Assign this in the Inspector!
    // --- End Specific Prize Info field ---

    [Header("Sound Effects")]
    [SerializeField] private AudioClip popInSound;
    [SerializeField] private AudioClip popOutSound;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private float spinTime = 2f; // Spin duration in seconds
    [SerializeField] private float spinDegrees = 320f; // Total degrees to spin

    private void Start()
    {
        // Ensure initial state is hidden
        if (popCan != null) popCan.SetActive(false);
        if (modelRoot != null) modelRoot.SetActive(false);
        if (popBurst != null) popBurst.SetActive(false);
        if (popText != null) popText.SetActive(false);

        // Basic check for sfxSource
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                Debug.LogWarning("TriggerProto: No AudioSource found on this GameObject. Sound effects will not play.", this);
            }
        }
    }

    public void ActivateModelPopup()
    {
        if (prizeToPopup == null)
        {
            Debug.LogError("TriggerProto: 'Prize To Popup' field is not assigned in the Inspector! Cannot activate popup.", this);
            return;
        }

        // --- CHANGE THIS LINE ---
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.StartManagedCoroutine(ModelPopup(prizeToPopup));
        }
        else
        {
            Debug.LogError("CollectionManager.Instance is null! Cannot start ModelPopup coroutine.", this);
            // Fallback: If CollectionManager isn't available, try to start it on this if it's active.
            // This makes the original problem resurface, but it's a good fallback for non-persistent cases.
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ModelPopup(prizeToPopup));
            }
            else
            {
                Debug.LogError("TriggerProto GameObject is inactive. Cannot start coroutine even as fallback.");
            }
        }
        // --- END CHANGE ---
    }


    private IEnumerator ModelPopup(PrizeInfo prizeInfo)
    {
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.AddPrize(prizeInfo.prizeID);
            Debug.Log($"Added prize: {prizeInfo.prizeID} to collection.");
        }
        else
        {
            Debug.LogWarning("CollectionManager.Instance not found. Prize not added to collection.", this);
        }

        // Play pop-in sound
        if (SoundFXManager.Instance != null && popInSound != null)
        {
            SoundFXManager.Instance.PlaySFXClip(popInSound, transform, 0.8f);
        }
        else if (sfxSource != null && popInSound != null) // Fallback if SoundFXManager is not used
        {
            sfxSource.PlayOneShot(popInSound, 0.8f);
        }


        yield return new WaitForSeconds(0.2f); // Small delay for effect

        // Activate parent GameObjects
        if (popCan != null) popCan.SetActive(true);
        if (popText != null) popText.SetActive(true);
        if (popBurst != null) popBurst.SetActive(true);
        if (modelRoot != null) modelRoot.SetActive(true);

        // Instantiate the model
        if (prizeInfo.model != null && modelRoot != null)
        {
            currentModel = Instantiate(prizeInfo.model, modelRoot.transform);
            currentModel.transform.localScale = Vector3.one * 1f; // Adjust scale here

            if (prizeInfo.ud == true) // Assuming 'ud' means something specific for positioning
            {
                currentModel.transform.localPosition = new Vector3(0.25f, 3.6f, 0f); // Adjust positioning
            }
            else
            {
                currentModel.transform.localPosition = Vector3.zero; // Default positioning
            }
        }
        else
        {
            Debug.LogError("TriggerProto: Prize model or modelRoot is null. Cannot instantiate model for popup.", this);
        }


        // Start scaling animations for various elements
        if (modelRoot != null) StartCoroutine(ScalePopIn(modelRoot.transform, 0.3f));
        if (popBurst != null) StartCoroutine(ScalePopIn(popBurst.transform, 0.3f));
        if (popText != null) StartCoroutine(ScalePopIn(popText.transform, 0.3f));

        // Start spin animation
        if (popBurst != null) StartCoroutine(SpinCanvas(spinTime));

        yield return new WaitForSeconds(spinTime - 0.3f); // Wait until just before spin ends for fade out

        // Play pop-out sound
        if (SoundFXManager.Instance != null && popOutSound != null)
        {
            SoundFXManager.Instance.PlaySFXClip(popOutSound, transform, 1.2f);
        }
        else if (sfxSource != null && popOutSound != null) // Fallback
        {
            sfxSource.PlayOneShot(popOutSound, 1.2f);
        }

        yield return new WaitForSeconds(0.3f); // Wait for pop-out sound to finish playing

        // Start scaling out animations
        if (modelRoot != null) StartCoroutine(ScalePopOut(modelRoot.transform, 0.2f));
        if (popBurst != null) StartCoroutine(ScalePopOut(popBurst.transform, 0.2f));
        if (popText != null) StartCoroutine(ScalePopOut(popText.transform, 0.2f));

        yield return new WaitForSeconds(0.2f); // Wait for pop-out animation to finish

        // Clean up: Destroy instantiated model and deactivate all popup elements
        if (currentModel != null) Destroy(currentModel);
        if (popCan != null) popCan.SetActive(false);
        if (popText != null) popText.SetActive(false);
        if (popBurst != null) popBurst.SetActive(false);
        if (modelRoot != null) modelRoot.SetActive(false);

        yield return new WaitForSeconds(0.5f); // Final short delay before full return
    }

    private IEnumerator SpinCanvas(float duration)
    {
        if (popBurst == null) yield break; // Safety check
        float elapsed = 0f;
        float speed = spinDegrees / duration; // degrees per second

        while (elapsed < duration)
        {
            popBurst.transform.Rotate(0f, 0f, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Optional: reset rotation if needed
        popBurst.transform.localRotation = Quaternion.identity;
    }

    private IEnumerator ScalePopIn(Transform target, float duration)
    {
        if (target == null) yield break; // Safety check
        Vector3 initialScale = target.localScale; // Store initial scale before setting to zero
        target.localScale = Vector3.zero;

        float overshootDuration = duration * 0.6f;
        float settleDuration = duration * 0.4f;

        // Phase 1: Grow from 0 to 120% of original scale
        float elapsed = 0f;
        while (elapsed < overshootDuration)
        {
            float t = elapsed / overshootDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, initialScale * 1.2f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Shrink from 120% back to 100%
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            float t = elapsed / settleDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            target.localScale = Vector3.LerpUnclamped(initialScale * 1.2f, initialScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = initialScale; // Ensure final scale is exact
    }

    private IEnumerator ScalePopOut(Transform target, float duration)
    {
        if (target == null) yield break; // Safety check
        Vector3 originalScale = target.localScale;
        float shrinkDuration = duration;

        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            target.localScale = Vector3.LerpUnclamped(originalScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = Vector3.zero;
    }
}