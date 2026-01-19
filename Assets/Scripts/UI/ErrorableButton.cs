using System.Collections;
using UnityEngine;
using TMPro;

public class ErrorableButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private CanvasGroup errorCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float slideDistance = 20f;
    [SerializeField] private float animDuration = 0.1f;

    Coroutine flashRoutine;
    Vector2 hiddenPos;
    Vector2 shownPos;

    void Awake()
    {
        RectTransform rt = errorText.rectTransform;
        shownPos = rt.anchoredPosition;
        hiddenPos = shownPos + Vector2.up * slideDistance;

        errorText.gameObject.SetActive(false);
        errorCanvasGroup.alpha = 0f;
    }

    public void FlashError(string message, float duration)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashErrorRoutine(message, duration));
    }

    IEnumerator FlashErrorRoutine(string message, float duration)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        // Show
        yield return Animate(
            fromPos: hiddenPos,
            toPos: shownPos,
            fromAlpha: 0f,
            toAlpha: 1f
        );

        yield return new WaitForSeconds(duration);

        // Hide
        yield return Animate(
            fromPos: shownPos,
            toPos: hiddenPos,
            fromAlpha: 1f,
            toAlpha: 0f
        );

        errorText.gameObject.SetActive(false);
        flashRoutine = null;
    }

    IEnumerator Animate(
        Vector2 fromPos,
        Vector2 toPos,
        float fromAlpha,
        float toAlpha
    )
    {
        RectTransform rt = errorText.rectTransform;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / animDuration);
            float alphaLerip = Mathf.Clamp01(t / (animDuration/2));

            rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, lerp);
            errorCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, lerp);

            yield return null;
        }

        rt.anchoredPosition = toPos;
        errorCanvasGroup.alpha = toAlpha;
    }
}
