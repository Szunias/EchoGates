using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int hitsToKill = 3;          // ← ile trafień wroga zabija
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    private int hitsTaken = 0;

    public void TakeHit()
    {
        hitsTaken++;
        if (audioSource && hitClip) audioSource.PlayOneShot(hitClip);

        if (hitsTaken >= hitsToKill)
        {
            if (audioSource && deathClip) audioSource.PlayOneShot(deathClip);
            Destroy(gameObject, 0.1f);                     // mała pauza → słychać dźwięk
        }
    }
}
