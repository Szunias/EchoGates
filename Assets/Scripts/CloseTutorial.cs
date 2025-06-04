using Unity.VisualScripting;
using UnityEngine;
using System.Collections; 

public class CloseTutorial : MonoBehaviour
{
    public FlickeringLight flickeringLight; // Drag in the Inspector
    public SubtitleManager subtitleManager;  // Drag your SubtitleManager here
    public string[] followUpSubtitles;     
    public float delayBetweenSubtitles = 3f;
    public void CloseUi()
    {
        FindFirstObjectByType<PlayerMovement>().Intutorial = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        if (flickeringLight != null)
        {
            flickeringLight.StartFlicker();
        }
        Destroy(gameObject);

        // Start subtitles
        if (subtitleManager != null && followUpSubtitles.Length > 0)
        {
            subtitleManager.StartCoroutine(ShowFollowUpSubtitles());
        } 
    }

    private IEnumerator ShowFollowUpSubtitles()
    {
        foreach (string subtitle in followUpSubtitles)
        {
            subtitleManager.ShowSubtitle(subtitle);
            yield return new WaitForSeconds(delayBetweenSubtitles);
        }
    }

}
