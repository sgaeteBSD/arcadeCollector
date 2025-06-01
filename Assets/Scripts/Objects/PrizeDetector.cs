using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;



public class PrizeDetector : MonoBehaviour
{
    public GameObject popCan;
    public GameObject popBurst;
    public GameObject popText;
    public GameObject modelRoot;
    private GameObject currentModel;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip popInSound;
    [SerializeField] private AudioClip popOutSound;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private float spinTime = 2f; // Spin duration in seconds
    [SerializeField] private float spinDegrees = 720f; // Total degrees to spin

    private void Start()
    {
        popCan.SetActive(false);
        modelRoot.SetActive(false);
        popBurst.SetActive(false);
        popText.SetActive(false);
    }
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
                StartCoroutine(ModelPopup(prizeInfo));
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
    private IEnumerator ModelPopup(PrizeInfo prizeInfo)
    {
        CollectionManager.Instance.AddPrize(prizeInfo.prizeID);
        SoundFXManager.Instance.PlaySFXClip(popInSound, transform, 0.8f);

        yield return new WaitForSeconds(0.2f);
        popCan.SetActive(true);
        popText.SetActive(true);
        popBurst.SetActive(true);
        modelRoot.SetActive(true);
        currentModel = Instantiate(prizeInfo.model, modelRoot.transform);
        currentModel.transform.localScale = Vector3.one * 1f; //adjust scale here
        currentModel.transform.localScale = Vector3.one * 1f; //adjust scale here
        if (prizeInfo.ud == true)
        {
            currentModel.transform.localPosition = new Vector3(0.25f, 3.6f, 0f); //adjust positioning
        }
        else
        {
            currentModel.transform.localPosition = Vector3.zero; //adjust positioning
        }

        StartCoroutine(ScalePopIn(modelRoot.transform, 0.3f));
        StartCoroutine(ScalePopIn(popBurst.transform, 0.3f));
        StartCoroutine(ScalePopIn(popText.transform, 0.3f));

        StartCoroutine(SpinCanvas(spinTime));
        yield return new WaitForSeconds(spinTime-0.3f);

        SoundFXManager.Instance.PlaySFXClip(popOutSound, transform, 1.2f);

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(ScalePopOut(modelRoot.transform, 0.2f));
        StartCoroutine(ScalePopOut(popBurst.transform, 0.2f));
        StartCoroutine(ScalePopOut(popText.transform, 0.2f));

        yield return new WaitForSeconds(0.2f);

        Destroy(currentModel);
        popCan.SetActive(false);
        popText.SetActive(false);
        popBurst.SetActive(false);
        modelRoot.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        SoundFXManager.Instance.StopMusic();

        FadeManager.Instance.FadeToScene("FreeRoam");
    }

    private IEnumerator SpinCanvas(float duration)
    {
        float elapsed = 0f;
        float speed = spinDegrees / duration; // degrees per second

        while (elapsed < duration)
        {
            popBurst.transform.Rotate(0f, 0f, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Optional: reset rotation if needed
        popBurst.transform.rotation = Quaternion.identity;
    }

    private IEnumerator ScalePopIn(Transform target, float duration)
    {
        Vector3 targetScale = target.localScale;
        target.localScale = Vector3.zero;

        float overshootDuration = duration * 0.6f;
        float settleDuration = duration * 0.4f;

        // Phase 1: Grow from 0 to 120% of original scale
        float elapsed = 0f;
        while (elapsed < overshootDuration)
        {
            float t = elapsed / overshootDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale * 1.2f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Shrink from 120% back to 100%
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            float t = elapsed / settleDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out
            target.localScale = Vector3.LerpUnclamped(targetScale * 1.2f, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = targetScale;
    }

    private IEnumerator ScalePopOut(Transform target, float duration)
    {
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