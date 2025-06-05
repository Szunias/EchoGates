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

    // ZNAJD� T� METOD�
    private IEnumerator PlaySubtitlesSequence()
    {
        // ZMIE� JEJ ZAWARTO�� NA PONI�SZ�:
        foreach (string line in totemSubtitles)
        {
            // U�ywamy nowej, globalnej metody
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(line);
            }
        }
        // Tutaj r�wnie� usuwamy yield return, aby nie blokowa� logiki.
        yield break;
    }
}
