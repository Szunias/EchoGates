using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class RaycastBeam : MonoBehaviour
{
    public Camera playerCamera;
    public Transform beamOrigin;
    
    [Tooltip("Range of the beam")] [SerializeField] public float soulRange = 50f;
    [Tooltip("Time between beam shots")][SerializeField] public float fireRate = 0.2f;
    [Tooltip("How long the soul activates its beam")] [SerializeField] public float beamDuration = 0.05f;

    LineRenderer beamLine;
    float fireTimer;

    private void Awake()
    {
        beamLine = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        if(Input.GetButtonDown("Fire1") && fireTimer > fireRate) //Fire1 is equal to right click
        {
            fireTimer = 0;
            beamLine.SetPosition(0,beamOrigin.position);
            Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, soulRange))
            {
                beamLine.SetPosition(1, hit.point);
               
            }
            else
            {
                beamLine.SetPosition(1, rayOrigin + (playerCamera.transform.forward * soulRange));
            }
            StartCoroutine(ShootBeam());
        }
    }

    IEnumerator ShootBeam()
    {
        beamLine.enabled = true;
        yield return new WaitForSeconds(beamDuration);
        beamLine.enabled = false;
    }

}
