using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public Image backgroundImage;

    public float textDisplayTime = 4f;
    public float initialDelay = 2f;

    private Coroutine subtitleRoutine;

    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (subtitleRoutine != null)
            StopCoroutine(subtitleRoutine);

        subtitleRoutine = StartCoroutine(SubtitleRoutine(text, duration > 0 ? duration : textDisplayTime));
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        yield return new WaitForSeconds(initialDelay);

        subtitleText.text = text;
        subtitleText.alpha = 1f;
        subtitleText.gameObject.SetActive(true);

        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0.5f; // or whatever you prefer
            backgroundImage.color = bgColor;
            backgroundImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);

        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(false);
    }
}