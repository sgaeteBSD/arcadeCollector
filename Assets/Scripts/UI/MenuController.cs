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
    public float animationDuration = 0.5f; public AnimationCurve easeCurve;
    [Header("Camera and Model Offsets")]
    public Camera uiCamera;
    public Vector3 modelOffsetFromCameraVisible;
    public Vector3 modelOffsetFromCameraHidden;

    private RectTransform menuRectTransform;
    private Vector2 menuHiddenPosition;
    private Vector2 menuVisiblePosition;
    private Coroutine currentAnimation;

    void Awake()
    {
        menuRectTransform = menuCanvas.GetComponent<RectTransform>();

        menuVisiblePosition = menuRectTransform.anchoredPosition;

        menuHiddenPosition = new Vector2(menuVisiblePosition.x, -menuRectTransform.rect.height * 1.5f);

        menuRectTransform.anchoredPosition = menuHiddenPosition;
        menuCanvas.SetActive(false);

        if (uiCamera == null)
        {
            Debug.LogError("UI Camera not assigned! Assign the camera that your UI uses to the 'UI Camera' field on MenuController.");
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
        modelRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool isOpening = !menuCanvas.activeSelf;

            if (isOpening)
            {
                menuCanvas.SetActive(true);
                modelRoot.SetActive(true);
                GameController.Instance.SetGameState(GameState.Menu);
                AnimateElements(true); SoundFXManager.Instance.PlaySFXClip(popInSound, transform, 1.3f);
            }
            else
            {
                GameController.Instance.SetGameState(GameState.FreeRoam);
                AnimateElements(false); SoundFXManager.Instance.PlaySFXClip(popOutSound, transform, 1.3f);
            }
        }
    }

    void AnimateElements(bool opening)
    {
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
            yield break;
        }

        float timer = 0f;

        Vector2 menuStartPos = menuRectTransform.anchoredPosition;
        Vector2 menuEndPos = opening ? menuVisiblePosition : menuHiddenPosition;

        Vector3 modelOffsetStart = opening ? modelOffsetFromCameraHidden : modelOffsetFromCameraVisible;
        Vector3 modelOffsetEnd = opening ? modelOffsetFromCameraVisible : modelOffsetFromCameraHidden;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float easedProgress = easeCurve.Evaluate(progress);

            menuRectTransform.anchoredPosition = Vector2.Lerp(menuStartPos, menuEndPos, easedProgress);

            Vector3 currentOffset = Vector3.Lerp(modelOffsetStart, modelOffsetEnd, easedProgress);
            modelRoot.transform.position = uiCamera.transform.position + currentOffset;

            yield return null;
        }

        menuRectTransform.anchoredPosition = menuEndPos;
        modelRoot.transform.position = uiCamera.transform.position + modelOffsetEnd;
        if (!opening)
        {
            menuCanvas.SetActive(false);
            modelRoot.SetActive(false);
        }
        currentAnimation = null;
    }
}