using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Hit points")]
    [SerializeField] private int hitsToKill = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    private int hitsTaken;
    private bool isDead;                                             // ★

    public void TakeHit()
    {
        if (isDead) return;                                          // ★

        hitsTaken++;

        // –– zwykły cios (nie‑śmiertelny) ––                          // ★
        if (hitsTaken < hitsToKill)
        {
            if (audioSource && hitClip)
                audioSource.PlayOneShot(hitClip);
            return;
        }

        // –– ŚMIERTELNY cios ––                                       // ★
        isDead = true;                                               // ★

        // 1. tworzę tymczasowy GO z AudioSource...
        if (deathClip)
        {
            GameObject temp = new GameObject("DeathSound");
            temp.transform.position = transform.position;

            AudioSource tmpSrc = temp.AddComponent<AudioSource>();
            if (audioSource)                                         // sklonuj najważniejsze parametry
            {
                tmpSrc.spatialBlend = audioSource.spatialBlend;
                tmpSrc.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
                tmpSrc.minDistance = audioSource.minDistance;
                tmpSrc.maxDistance = audioSource.maxDistance;
                tmpSrc.rolloffMode = audioSource.rolloffMode;
            }
            tmpSrc.clip = deathClip;
            tmpSrc.Play();
            Destroy(temp, deathClip.length);                         // auto‑sprzątanie
        }

        // 2. natychmiast usuń wizualną część wroga
        Destroy(gameObject);                                         // to samo tempo co wcześniej
    }
}
