using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private SubtitleManager subtitleManager;

    void Start()
    {
        StartCoroutine(PlayTutorialSequence());
    }

    private IEnumerator PlayTutorialSequence()
    {
        subtitleManager.ShowSubtitle("What is that on the table?");
        yield return new WaitForSeconds(1.5f);
        subtitleManager.ShowSubtitle("Is that... Captain Sniffles?");
        yield return new WaitForSeconds(1.5f);
    }
}
