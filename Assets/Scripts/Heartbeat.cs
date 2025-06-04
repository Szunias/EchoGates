using Unity.VisualScripting;
using UnityEngine;


public class Heartbeat : MonoBehaviour
{
    private AudioSource source;
    private Vector3 previousSpiderPosition;
    private Vector3 previousPlayerPosition;

    [SerializeField] public GameObject spider;
    
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 currentSpiderPosition = spider.transform.position;
        float distanceFromSpider = Vector3.Distance(currentPosition, currentSpiderPosition);
        source.pitch = Mathf.Lerp(source.pitch, GetPitch(distanceFromSpider), Time.deltaTime * 5f);
        previousPlayerPosition = currentPosition;
        previousSpiderPosition = currentSpiderPosition;
    }

    private float GetPitch(float distance)
    {
        float t = 1f - Mathf.Clamp01(distance / 40f);
        return Mathf.Lerp(0.6f, 2f, t);
    }
}
