using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        fadeImage.raycastTarget = false;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional
    }

    public void FadeToScene(string sceneName)
    {
    this.gameObject.SetActive(true);
    StartCoroutine(FadeAndLoadScene(sceneName));
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0f, 1f)); // Transparent to black
    }

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f)); // Black to transparent
        PlayerController.Instance.leaving = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        fadeImage.raycastTarget = true;
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
        fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(sceneName);
        yield return null; // wait one frame for scene to load

        yield return StartCoroutine(FadeIn());
    }
}
