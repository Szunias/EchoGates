using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public float textDisplayTime = 4f;

    private Coroutine subtitleRoutine;

    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (subtitleRoutine != null)
            StopCoroutine(subtitleRoutine);

        subtitleRoutine = StartCoroutine(SubtitleRoutine(text, duration > 0 ? duration : textDisplayTime));
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }
} 
