using UnityEngine;

public class Footsteps : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField] AudioClip[] _clips;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        AudioClip clip = _clips[Random.Range(0, _clips.Length)];
        _audioSource.clip = clip;
        _audioSource.pitch -= Time.deltaTime * Random.Range(0, 4) / 5;
        _audioSource.Play();

        if (!_audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!_audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
