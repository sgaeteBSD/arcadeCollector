using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject modelRoot;

    [SerializeField] private AudioClip popInSound;
    [SerializeField] private AudioClip popOutSound;
    [SerializeField] private AudioClip flipSound;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f; // How long the animation takes
    public AnimationCurve easeCurve; // To control the easing (e.g., EaseInOut)

    [Header("Camera and Model Offsets")]
    public Camera uiCamera; // Assign the Camera your UI uses (usually Main Camera)

    // These define where the model should be RELATIVE to the camera's position
    // Set these in the Inspector!
    public Vector3 modelOffsetFromCameraVisible;
    public Vector3 modelOffsetFromCameraHidden;

    private RectTransform menuRectTransform;
    private Vector2 menuHiddenPosition;
    private Vector2 menuVisiblePosition;
    private Coroutine currentAnimation;

    void Awake()
    {
        // --- UI Canvas Setup ---
        menuRectTransform = menuCanvas.GetComponent<RectTransform>();

        // We assume the menuCanvas is set up in the UI at its desired "visible" position
        // in the editor.
        menuVisiblePosition = menuRectTransform.anchoredPosition;

        // Calculate the hidden position for the UI menu (below the screen)
        // This calculation is for Screen Space UI. Adjust if your UI anchoring is different.
        menuHiddenPosition = new Vector2(menuVisiblePosition.x, -menuRectTransform.rect.height * 1.5f);

        // Set UI to its initial hidden state
        menuRectTransform.anchoredPosition = menuHiddenPosition;
        menuCanvas.SetActive(false);

        // --- Model Root Setup ---
        // Ensure modelRoot is set to its initial hidden state relative to the camera
        // It's important that uiCamera is assigned before Awake runs.
        if (uiCamera == null)
        {
            Debug.LogError("UI Camera not assigned! Assign the camera that your UI uses to the 'UI Camera' field on MenuController.");
            // Try to find the main camera as a fallback, though explicit assignment is better.
            uiCamera = Camera.main;
            if (uiCamera == null)
            {
                Debug.LogError("No main camera found! Model will not initialize correctly without a UI Camera reference.");
            }
        }

        if (uiCamera != null)
        {
            modelRoot.transform.position = uiCamera.transform.position + modelOffsetFromCameraHidden;
        }
        modelRoot.SetActive(false); // Make sure it starts inactive
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool isOpening = !menuCanvas.activeSelf;

            if (isOpening)
            {
                // Activate both before starting animation
                menuCanvas.SetActive(true);
                modelRoot.SetActive(true);
                GameController.Instance.SetGameState(GameState.Menu);
                AnimateElements(true); // Animate in
                SoundFXManager.Instance.PlaySFXClip(popInSound, transform, 1.3f);
            }
            else
            {
                GameController.Instance.SetGameState(GameState.FreeRoam);
                AnimateElements(false); // Animate out
                SoundFXManager.Instance.PlaySFXClip(popOutSound, transform, 1.3f);
            }
        }
    }

    void AnimateElements(bool opening)
    {
        // Stop any ongoing animation to prevent conflicts
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateElementsCoroutine(opening));
    }

    IEnumerator AnimateElementsCoroutine(bool opening)
    {
        if (uiCamera == null)
        {
            Debug.LogError("UI Camera is null during animation! Model will not animate correctly.");
            yield break; // Exit coroutine if camera is missing
        }

        float timer = 0f;

        // UI Canvas animation variables
        Vector2 menuStartPos = menuRectTransform.anchoredPosition;
        Vector2 menuEndPos = opening ? menuVisiblePosition : menuHiddenPosition;

        // Model Root animation offsets
        Vector3 modelOffsetStart = opening ? modelOffsetFromCameraHidden : modelOffsetFromCameraVisible;
        Vector3 modelOffsetEnd = opening ? modelOffsetFromCameraVisible : modelOffsetFromCameraHidden;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float easedProgress = easeCurve.Evaluate(progress);

            // Animate UI Canvas (still based on anchored position)
            menuRectTransform.anchoredPosition = Vector2.Lerp(menuStartPos, menuEndPos, easedProgress);

            // Animate Model Root relative to the camera's current position
            Vector3 currentOffset = Vector3.Lerp(modelOffsetStart, modelOffsetEnd, easedProgress);
            modelRoot.transform.position = uiCamera.transform.position + currentOffset;

            yield return null; // Wait for the next frame
        }

        // Ensure elements end precisely at their target positions
        menuRectTransform.anchoredPosition = menuEndPos;
        modelRoot.transform.position = uiCamera.transform.position + modelOffsetEnd; // Final position relative to camera

        // If closing, deactivate elements after the animation is complete
        if (!opening)
        {
            menuCanvas.SetActive(false);
            modelRoot.SetActive(false);
        }
        currentAnimation = null; // Clear the coroutine reference
    }
}