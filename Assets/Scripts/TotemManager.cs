using UnityEngine;
using System.Collections;

public class TotemManager : MonoBehaviour
{
    [SerializeField] private TutorialTotem totem1;
    [SerializeField] private TutorialTotem totem2;
    [SerializeField] private SubtitleManager subtitleManager;

    [TextArea(2, 5)] public string[] totemSubtitles;

    private bool subtitlesPlayed = false;

    void Update()
    {
        if (!subtitlesPlayed && totem1.IsLit && totem2.IsLit)
        {
            subtitlesPlayed = true;
            StartCoroutine(PlaySubtitlesSequence());
        }
    }

    private IEnumerator PlaySubtitlesSequence()
    {
        foreach (string line in totemSubtitles)
        {
            subtitleManager.ShowSubtitle(line); // using your existing manager
            yield return new WaitForSeconds(subtitleManager.textDisplayTime + 0.5f);
        }
    }
}
