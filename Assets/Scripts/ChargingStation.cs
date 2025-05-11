using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ChargingStation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's GameObject that holds/represents the cube in hand (scene object). This object will be deactivated when the cube is on the station.")]
    [SerializeField] private GameObject playerCubeInHand;
    [Tooltip("The EnergyController component on the player to refill energy")]
    [SerializeField] private EnergyController playerEnergy;
    [Tooltip("The Light component on this Charging Station that will activate during charging.")]
    [SerializeField] private Light stationLight; // NOWA REFERENCJA

    [Header("Prefabs & Points")]
    [Tooltip("Prefab of the cube to place on station")]
    [SerializeField] private GameObject cubePrefabOnStation;
    [Tooltip("Transform where the cube should appear on station")]
    [SerializeField] private Transform stationCubeSpawnPoint;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [Tooltip("Extra energy rewarded when picking up a fully charged cube.")]
    [SerializeField] private float energyRewardOnPickup = 25f;
    [Tooltip("Energy restored per second while charging.")]
    [SerializeField] private float chargeSpeed = 10f;
    [Tooltip("Intensity of the station light when charging.")]
    [SerializeField] private float chargingLightIntensity = 2f; // NOWA ZMIENNA

    private GameObject _instantiatedStationCube;
    private Coroutine _chargeCoroutine;
    private bool _isCharging = false;
    private bool _playerInRange = false;
    private float _originalLightIntensity; // Do przechowywania oryginalnej intensywno�ci, je�li by�a > 0

    private void Awake()
    {
        Debug.Log("ChargingStation Awake: Validating references...");
        if (playerEnergy == null)
            Debug.LogError("Player Energy Controller not assigned on ChargingStation: " + gameObject.name, this);
        if (playerCubeInHand == null)
            Debug.LogError("Player Cube In Hand reference not assigned on ChargingStation: " + gameObject.name, this);
        if (cubePrefabOnStation == null)
            Debug.LogError("Cube Prefab On Station not assigned on ChargingStation: " + gameObject.name, this);
        if (stationCubeSpawnPoint == null)
            Debug.LogError("Station Cube Spawn Point not assigned on ChargingStation: " + gameObject.name, this);
        if (interactAction == null || interactAction.action == null)
            Debug.LogError("Interact Action not assigned on ChargingStation: " + gameObject.name, this);
        else
            Debug.Log("Interact Action is assigned.");

        // Walidacja i inicjalizacja �wiat�a stacji
        if (stationLight == null)
        {
            Debug.LogWarning("Station Light not assigned on ChargingStation: " + gameObject.name + ". Attempting to find it on this GameObject.", this);
            stationLight = GetComponent<Light>();
            if (stationLight == null)
            {
                Debug.LogError("Station Light component NOT FOUND on ChargingStation: " + gameObject.name + ". Please add a Light component and assign it.", this);
            }
        }

        if (stationLight != null)
        {
            _originalLightIntensity = stationLight.intensity; // Zapisz oryginaln� intensywno��
            stationLight.enabled = false; // Domy�lnie wy��cz �wiat�o stacji
            stationLight.intensity = 0;   // lub ustaw intensywno�� na 0
            Debug.Log("Station light initialized and turned off.");
        }
    }

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            Debug.Log("Enabling Interact Action.");
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
        else
        {
            Debug.LogError("Cannot enable interact action - it's null!");
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            Debug.Log("Disabling Interact Action.");
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
        StopChargingEffects();
        TurnOffStationLight(); // Upewnij si�, �e �wiat�o jest wy��czone przy deaktywacji
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("OnTriggerEnter: Player candidate entered range. Name: " + other.gameObject.name);
            EnergyController enteringPlayerEnergy = other.GetComponent<EnergyController>(); // Lub GetComponentInChildren je�li trzeba

            if (playerEnergy != null)
                Debug.Log("Inspected playerEnergy object: " + playerEnergy.gameObject.name);
            else
                Debug.LogWarning("Inspected playerEnergy is NULL!");


            if (enteringPlayerEnergy != null && enteringPlayerEnergy == playerEnergy)
            {
                _playerInRange = true;
                Debug.Log("Player CONFIRMED in range of Charging Station: " + other.gameObject.name);
            }
            else if (enteringPlayerEnergy == null)
            {
                Debug.LogWarning("Player tagged object (" + other.gameObject.name + ") entered range, but has no EnergyController component.");
            }
            else
            {
                Debug.LogWarning("Player tagged object (" + other.gameObject.name + ") entered range with an EnergyController, BUT it's NOT the same instance as 'playerEnergy' assigned in the Inspector. Assigned: " + (playerEnergy != null ? playerEnergy.gameObject.name : "NULL") + ", Detected: " + enteringPlayerEnergy.gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("OnTriggerExit: Player candidate exited range.");
            EnergyController exitingPlayerEnergy = other.GetComponent<EnergyController>(); // Lub GetComponentInChildren
            if (exitingPlayerEnergy != null && exitingPlayerEnergy == playerEnergy)
            {
                _playerInRange = false;
                Debug.Log("Player CONFIRMED exited range of Charging Station.");
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("--- INTERACT ACTION PERFORMED ---");

        if (!_playerInRange)
        {
            Debug.Log("Interact failed: Player not in range. _playerInRange is false.");
            return;
        }
        if (playerEnergy == null)
        {
            Debug.Log("Interact failed: playerEnergy reference is null.");
            return;
        }

        Debug.Log($"Current state: _isCharging = {_isCharging}");

        if (!_isCharging)
        {
            Debug.Log("Attempting to start charging (calling TryStartCharging)...");
            TryStartCharging();
        }
        else
        {
            Debug.Log("Already charging. Checking if energy is full to retrieve...");
            // Pozw�l graczowi odzyska� kostk�, nawet je�li nie jest w pe�ni na�adowana,
            // ale �wiat�o i nagroda b�d� zale�e� od stanu na�adowania.
            // if (playerEnergy.Percent >= playerEnergy.MaxEnergy) // Usuni�to ten warunek, by zawsze mo�na by�o spr�bowa� zabra�
            // {
            RetrieveChargedCube();
            // }
            // else
            // {
            //    Debug.Log("Charging in progress. Press E again to retrieve cube (not fully charged).");
            // }
        }
    }

    private void TryStartCharging()
    {
        Debug.Log("TryStartCharging called.");

        if (playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            Debug.Log("TryStartCharging Aborted: Player energy is already full.");
            return;
        }

        if (playerCubeInHand == null)
        {
            Debug.Log("TryStartCharging Aborted: playerCubeInHand reference is null.");
            return;
        }

        if (!playerCubeInHand.activeSelf)
        {
            Debug.Log("TryStartCharging Aborted: playerCubeInHand GameObject is INACTIVE. Player isn't 'holding' a cube to place.");
            return;
        }

        if (cubePrefabOnStation == null || stationCubeSpawnPoint == null)
        {
            Debug.LogError("TryStartCharging Aborted: Cube Prefab On Station or Station Cube Spawn Point is not set.", this);
            return;
        }

        Debug.Log("All checks in TryStartCharging passed. Proceeding to place cube...");

        playerCubeInHand.SetActive(false);
        Debug.Log("Player's cube in hand deactivated.");

        if (_instantiatedStationCube != null) Destroy(_instantiatedStationCube);
        _instantiatedStationCube = Instantiate(cubePrefabOnStation, stationCubeSpawnPoint.position, stationCubeSpawnPoint.rotation, stationCubeSpawnPoint);
        Debug.Log("New cube instantiated on station at " + stationCubeSpawnPoint.name);

        _isCharging = true;
        TurnOnStationLight(); // W��CZ �WIAT�O
        Debug.Log("Charging started. _isCharging set to true. Station light turned ON.");
        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _chargeCoroutine = StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        Debug.Log($"ChargeRoutine started. Current Energy: {playerEnergy.Percent}/{playerEnergy.MaxEnergy}");
        while (_isCharging && playerEnergy.Percent < playerEnergy.MaxEnergy) // Dodano warunek _isCharging
        {
            playerEnergy.AddEnergy(chargeSpeed * Time.deltaTime);
            // Opcjonalnie: dynamiczna zmiana �wiat�a podczas �adowania
            // if (stationLight != null && stationLight.enabled)
            // {
            //     stationLight.intensity = Mathf.Lerp(0, chargingLightIntensity, playerEnergy.Percent / playerEnergy.MaxEnergy);
            // }
            yield return null;
        }

        if (playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            Debug.Log("Charging complete. Player energy is full.");
            // Opcjonalnie: zmie� �wiat�o na "pe�ne na�adowanie", np. ja�niejsze lub inny kolor
            // if (stationLight != null) stationLight.intensity = chargingLightIntensity * 1.5f; // Przyk�ad
        }
        else if (!_isCharging)
        {
            Debug.Log("ChargeRoutine interrupted because _isCharging became false (cube likely retrieved early).");
        }
        _chargeCoroutine = null;
    }

    private void RetrieveChargedCube()
    {
        Debug.Log("RetrieveChargedCube called.");
        // Usuni�to warunek !_isCharging, bo teraz zawsze mo�na pr�bowa� zabra�
        // if (!_isCharging && _instantiatedStationCube == null)
        if (_instantiatedStationCube == null) // Wystarczy sprawdzi� czy jest kostka na stacji
        {
            Debug.Log("RetrieveChargedCube Aborted: No cube on station to retrieve.");
            return;
        }

        if (playerCubeInHand == null)
        {
            Debug.LogError("RetrieveChargedCube Aborted: Player Cube In Hand reference is missing!", this);
            return;
        }

        bool wasFullyCharged = playerEnergy.Percent >= playerEnergy.MaxEnergy;

        StopChargingEffects(); // To zatrzyma korutyn� �adowania

        _isCharging = false; // Ustaw _isCharging na false PRZED zniszczeniem kostki i oddaniem graczowi
        TurnOffStationLight(); // WY��CZ �WIAT�O
        Debug.Log("Retrieval process started. _isCharging set to false. Station light turned OFF.");


        if (_instantiatedStationCube != null)
        {
            Destroy(_instantiatedStationCube);
            _instantiatedStationCube = null;
            Debug.Log("Station cube destroyed.");
        }

        playerCubeInHand.SetActive(true);
        Debug.Log("Player's cube in hand reactivated.");

        if (wasFullyCharged && playerEnergy != null) // Nagroda tylko je�li by�o w pe�ni na�adowane
        {
            playerEnergy.AddEnergy(energyRewardOnPickup);
            Debug.Log($"Cube retrieved (was fully charged). Energy reward of {energyRewardOnPickup} applied.");
        }
        else if (playerEnergy != null)
        {
            Debug.Log($"Cube retrieved (was not fully charged). No reward. Current Energy: {playerEnergy.Percent}");
        }
        // _isCharging = false; // Przeniesione wy�ej
        Debug.Log("Retrieval complete.");
    }

    private void StopChargingEffects()
    {
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
            Debug.Log("ChargeRoutine coroutine stopped.");
        }
    }

    private void TurnOnStationLight()
    {
        if (stationLight != null)
        {
            stationLight.enabled = true;
            stationLight.intensity = chargingLightIntensity;
            Debug.Log("Station light turned ON with intensity: " + chargingLightIntensity);
        }
        else
        {
            Debug.LogWarning("Attempted to turn on station light, but stationLight reference is null.");
        }
    }

    private void TurnOffStationLight()
    {
        if (stationLight != null)
        {
            stationLight.enabled = false;
            // Mo�esz chcie� przywr�ci� oryginaln� intensywno��, je�li by�a > 0 i chcesz, aby stacja mia�a s�abe �wiat�o, gdy nie �aduje
            // stationLight.intensity = _originalLightIntensity;
            // lub po prostu wyzerowa�:
            stationLight.intensity = 0;
            Debug.Log("Station light turned OFF.");
        }
        else
        {
            Debug.LogWarning("Attempted to turn off station light, but stationLight reference is null.");
        }
    }

    // Optional: If you want to visualize the spawn point in the editor
    private void OnDrawGizmosSelected()
    {
        if (stationCubeSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(stationCubeSpawnPoint.position, 0.25f);
            Gizmos.DrawLine(stationCubeSpawnPoint.position, stationCubeSpawnPoint.position + stationCubeSpawnPoint.forward * 0.5f);
        }
    }
}