using UnityEngine;
using UnityEngine.SceneManagement;

public class DogInteraction : MonoBehaviour
{

    [SerializeField] private string winSceneName = "GameWon";
    private bool isPlayerInRange = false;
 

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && TotemLightingUp.cageIsGone)
        {
            TriggerWin();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the cube.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the cube's range.");
        }
    }

    private void TriggerWin()
    {
        Debug.Log("Pressed E near the cube! Loading win scene...");
        SceneManager.LoadScene(winSceneName);
    }
}




