using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ChargingStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnergyController playerEnergy;   // your energy system
    [SerializeField] private GameObject cubeInHand;          // the real cube you hold
    [SerializeField] private GameObject cubeStationPrefab;   // prefab clone for the charger
    [SerializeField] private Transform spawnPoint;           // where on the pad to put the cube
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [SerializeField] private float energyReward = 25f;       // extra bonus on retrieve
    [SerializeField] private float chargeSpeed = 10f;      // energy/sec while charging

    [Header("UI Widget")]
    [Tooltip("A world-space Canvas prefab that has a Slider and a Text child")]
    [SerializeField] private GameObject widgetPrefab;
    [SerializeField] private Transform widgetSpawnPoint;     // empty GameObject above station

    private GameObject _stationCube;    // the spawned copy on the pad
    private bool _isCharging = false;
    private bool _playerInRange = false;

    // cached for restoring your real cube
    private Transform _origParent;
    private Vector3 _origLocalPos;
    private Quaternion _origLocalRot;

    // UI widget instance & its parts
    private GameObject _widgetInstance;
    private Slider _widgetSlider;
    private Text _widgetLabel;

    private void OnEnable()
    {
        if (interactAction?.action == null)
        {
            Debug.LogError("InteractAction not set!");
            return;
        }
        interactAction.action.performed += OnInteract;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction?.action == null) return;
        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = false;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_playerInRange) return;

        if (!_isCharging)
            PlaceCubeOnStation();
        else if (playerEnergy.Percent >= playerEnergy.MaxEnergy)
            RetrieveCube();
        else
            Debug.Log("Still charging…");
    }

    private void PlaceCubeOnStation()
    {
        if (cubeInHand == null || cubeStationPrefab == null || widgetPrefab == null || widgetSpawnPoint == null)
        {
            Debug.LogError("Assign cubeInHand, cubeStationPrefab, widgetPrefab and widgetSpawnPoint in inspector!");
            return;
        }

        // 1) cache original transform so we can snap back exactly
        _origParent = cubeInHand.transform.parent;
        _origLocalPos = cubeInHand.transform.localPosition;
        _origLocalRot = cubeInHand.transform.localRotation;

        // 2) hide your hand-cube
        cubeInHand.SetActive(false);

        // 3) spawn a clone on the station
        _stationCube = Instantiate(cubeStationPrefab, spawnPoint.position, spawnPoint.rotation);
        var stationLight = _stationCube.GetComponent<CubeLight>();
        if (stationLight != null) stationLight.enabled = false;

        // 4) create the UI widget above the pad
        _widgetInstance = Instantiate(
            widgetPrefab,
            widgetSpawnPoint.position,
            widgetSpawnPoint.rotation,
            widgetSpawnPoint
        );
        // find the slider + text inside it
        _widgetSlider = _widgetInstance.GetComponentInChildren<Slider>();
        _widgetLabel = _widgetInstance.GetComponentInChildren<Text>();
        // init to zero
        if (_widgetSlider != null) _widgetSlider.value = 0f;
        if (_widgetLabel != null) _widgetLabel.text = "0%";

        _isCharging = true;
        Debug.Log("Cube placed. Charging started!");
        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        while (playerEnergy.Percent < playerEnergy.MaxEnergy)
        {
            // top up
            playerEnergy.AddEnergy(Time.deltaTime * chargeSpeed);

            // update UI widget
            float pct = Mathf.Clamp01(playerEnergy.Percent / playerEnergy.MaxEnergy);
            if (_widgetSlider != null) _widgetSlider.value = pct;
            if (_widgetLabel != null) _widgetLabel.text = $"{Mathf.RoundToInt(pct * 100)}%";

            yield return null;
        }
        Debug.Log("Charging complete!");
    }

    private void RetrieveCube()
    {
        if (!_isCharging)
        {
            Debug.LogWarning("Nothing to retrieve!");
            return;
        }

        // 1) destroy station copy
        if (_stationCube != null) Destroy(_stationCube);
        // 2) destroy UI widget
        if (_widgetInstance != null) Destroy(_widgetInstance);

        // 3) restore your real cube into your hand exactly as before
        cubeInHand.SetActive(true);
        cubeInHand.transform.SetParent(_origParent, worldPositionStays: false);
        cubeInHand.transform.localPosition = _origLocalPos;
        cubeInHand.transform.localRotation = _origLocalRot;

        // 4) re-enable its glow
        var handLight = cubeInHand.GetComponent<CubeLight>();
        if (handLight != null) handLight.enabled = true;

        // 5) give reward
        playerEnergy.AddEnergy(energyReward);

        _isCharging = false;
        Debug.Log("Cube retrieved! Bonus energy awarded.");
    }
}
