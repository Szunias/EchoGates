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

    // ZNAJDè T  METOD 
    private IEnumerator PlaySubtitlesSequence()
    {
        // ZMIE— JEJ ZAWARTOå∆ NA PONIØSZ•:
        foreach (string line in totemSubtitles)
        {
            // Uøywamy nowej, globalnej metody
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(line);
            }
        }
        // Tutaj rÛwnieø usuwamy yield return, aby nie blokowaÊ logiki.
        yield break;
    }
}
