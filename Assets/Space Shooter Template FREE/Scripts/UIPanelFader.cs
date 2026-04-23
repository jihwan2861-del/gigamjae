using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIPanelFader : MonoBehaviour
{
    private Image fadeImage;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
    }

    // 화면을 서서히 어둡게 (Alpha: 0 -> targetAlpha)
    public void FadeToBlack(float duration = 2.0f, float targetAlpha = 0.8f)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(0f, targetAlpha, duration));
    }

    // 화면을 서서히 밝게 (Alpha: current -> 0)
    public void FadeFromBlack(float duration = 2.0f)
    {
        StartCoroutine(FadeRoutine(fadeImage.color.a, 0f, duration, true));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, bool disableAtEnd = false)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // 게임이 멈춰있어도 작동하게 unscaled 사용
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, endAlpha);
        if (disableAtEnd) gameObject.SetActive(false);
    }
}
