using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro; // WAØNE: Zamiast UnityEngine.UI

public class ChargingStation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's GameObject that holds/represents the cube in hand (scene object). This object will be deactivated when the cube is on the station.")]
    [SerializeField] private GameObject playerCubeInHand;
    [Tooltip("The EnergyController component on the player to refill energy")]
    [SerializeField] private EnergyController playerEnergy;
    [Tooltip("The Light component on this Charging Station that will activate during charging.")]
    [SerializeField] private Light stationLight;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;

    [Header("UI Notification (TextMeshPro)")] // Zmieniono nazwÍ sekcji dla jasnoúci
    [Tooltip("TextMeshProUGUI element to display notifications.")]
    [SerializeField] private TextMeshProUGUI notificationText; // WAØNE: Zmieniono typ na TextMeshProUGUI
    [Tooltip("How long the notification stays on screen (in seconds).")]
    [SerializeField] private float notificationDisplayTime = 2.5f;
    [Tooltip("Text displayed when cube is placed in the charger.")]
    [SerializeField] private string putInChargerMessage = "Cube placed in charger";
    [Tooltip("Text displayed when cube is retrieved from the charger.")]
    [SerializeField] private string retrievedFromChargerMessage = "Cube retrieved";
    [Tooltip("Text displayed when player has full energy")]
    [SerializeField] private string energyFullMessage = "Energy already full!";
    [Tooltip("Text displayed when player has no cube in hand")]
    [SerializeField] private string noCubeMessage = "No cube in hand to place!";
    [Tooltip("Text displayed when Cube fully charged")]
    [SerializeField] private string cubeFullyChargedMessage = "Cube fully charged!";
    [SerializeField] private string interactionPromptMessage = "Press E to Interact with Dog";

    [Header("UI References")]
    [Tooltip("The TextMeshProUGUI element to display the interaction prompt.")]
    [SerializeField] private TextMeshProUGUI promptText; // Assign your prompt TextMeshProUGUI element here


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
    [SerializeField] private float chargingLightIntensity = 2f;

    [Header("Charging Station Subtitles")]
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private string[] firstChargeSubtitleLines;
    [SerializeField] private float subtitleDelay = 3f; 

    private GameObject _instantiatedStationCube;
    private Coroutine _chargeCoroutine;
    private Coroutine _notificationCoroutine;
    private bool _isCharging = false;
    private bool _playerInRange = false;
    private float _originalLightIntensity;
    private bool isPlayerInRange = false;

    private bool firstChargeDone = false;

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
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DogInteraction: PromptText is not assigned. UI prompt will not be displayed.", this.gameObject);
        }

        // Walidacja TextMeshProUGUI
        if (notificationText == null)
        {
            Debug.LogError("Notification Text (TextMeshProUGUI) not assigned on ChargingStation: " + gameObject.name + ". Please assign it in the Inspector.", this);
        }
        else
        {
            notificationText.gameObject.SetActive(false); // Upewnij siÍ, øe tekst jest wy≥πczony na starcie
        }

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
            _originalLightIntensity = stationLight.intensity;
            stationLight.enabled = false;
            stationLight.intensity = 0;
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
        TurnOffStationLight();
        if (notificationText != null && notificationText.gameObject.activeSelf)
        {
            notificationText.gameObject.SetActive(false);
        }
        if (_notificationCoroutine != null)
        {
            StopCoroutine(_notificationCoroutine);
            _notificationCoroutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnergyController enteringPlayerEnergy = other.GetComponent<EnergyController>();
            if (enteringPlayerEnergy != null && enteringPlayerEnergy == playerEnergy)
            {
                _playerInRange = true;
                Debug.Log("Player CONFIRMED in range of Charging Station: " + other.gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnergyController exitingPlayerEnergy = other.GetComponent<EnergyController>();
            if (exitingPlayerEnergy != null && exitingPlayerEnergy == playerEnergy)
            {
                _playerInRange = false;
                Debug.Log("Player CONFIRMED exited range of Charging Station.");
            }
            if (promptText != null && promptText.gameObject.activeSelf)
            {
                promptText.gameObject.SetActive(false); // Hide prompt when player exits
            }
        }
    }

    private void Update()
    {
        if (_playerInRange)
        {
            if (promptText != null && !promptText.gameObject.activeSelf)
            {
                promptText.text = interactionPromptMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange || playerEnergy == null) return;

        if (!_isCharging)
        {
            TryStartCharging();
        }
        else
        {
            source.Stop();
            RetrieveChargedCube();
        }
    }

    private void TryStartCharging()
    {
        if (playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            ShowNotification(energyFullMessage);
            Debug.Log("TryStartCharging Aborted: Player energy is already full.");
            return;
        }
        if (playerCubeInHand == null || !playerCubeInHand.activeSelf)
        {
            ShowNotification(noCubeMessage);
            Debug.Log("TryStartCharging Aborted: playerCubeInHand is null or inactive.");
            return;
        }
        if (cubePrefabOnStation == null || stationCubeSpawnPoint == null)
        {
            Debug.LogError("TryStartCharging Aborted: Cube Prefab On Station or Station Cube Spawn Point is not set.", this);
            return;
        }

        playerCubeInHand.SetActive(false);
        if (_instantiatedStationCube != null) Destroy(_instantiatedStationCube);
        _instantiatedStationCube = Instantiate(cubePrefabOnStation, stationCubeSpawnPoint.position, stationCubeSpawnPoint.rotation, stationCubeSpawnPoint);

        _isCharging = true;
        source.clip = clip;
        source.Play();
        TurnOnStationLight();
        ShowNotification(putInChargerMessage);
        Debug.Log("Charging started. Cube placed in charger.");

        if (!firstChargeDone)
        {
            firstChargeDone = true;
            StartCoroutine(PlayFirstChargeSubtitles()); 
        } 

        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _chargeCoroutine = StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        Debug.Log($"ChargeRoutine started. Current Energy: {playerEnergy.Percent}/{playerEnergy.MaxEnergy}");
        while (_isCharging && playerEnergy.Percent < playerEnergy.MaxEnergy)
        {
            playerEnergy.AddEnergy(chargeSpeed * Time.deltaTime);
            yield return null;
        }

        if (playerEnergy.Percent >= playerEnergy.MaxEnergy && _isCharging) // Dodano _isCharging, aby komunikat nie pokazywa≥ siÍ po przerwaniu
        {
            Debug.Log("Charging complete. Player energy is full.");
            ShowNotification(cubeFullyChargedMessage);
        }
        else if (!_isCharging)
        {
            Debug.Log("ChargeRoutine interrupted (cube retrieved early or charging station disabled).");
        }
        _chargeCoroutine = null;
    }

    private void RetrieveChargedCube()
    {
        if (_instantiatedStationCube == null)
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

        // Najpierw zatrzymaj efekty ≥adowania i ustaw _isCharging na false
        // To zapobiegnie wyúwietleniu komunikatu "Cube fully charged!" jeúli gracz zabierze kostkÍ tuø przed pe≥nym na≥adowaniem
        StopChargingEffects();
        _isCharging = false; // Ustaw to PRZED potencjalnym wyúwietleniem komunikatu o pe≥nym na≥adowaniu w ChargeRoutine

        TurnOffStationLight();

        Destroy(_instantiatedStationCube);
        _instantiatedStationCube = null;
        playerCubeInHand.SetActive(true);

        if (wasFullyCharged)
        {
            playerEnergy.AddEnergy(energyRewardOnPickup);
            ShowNotification(retrievedFromChargerMessage + " (Fully Charged +Bonus)");
            Debug.Log($"Cube retrieved (fully charged). Energy reward applied.");
        }
        else
        {
            ShowNotification(retrievedFromChargerMessage);
            Debug.Log($"Cube retrieved (not fully charged).");
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText == null)
        {
            Debug.LogWarning("NotificationText (TextMeshProUGUI) is not assigned. Cannot display message: " + message);
            return;
        }

        if (_notificationCoroutine != null)
        {
            StopCoroutine(_notificationCoroutine);
        }
        _notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }

    private IEnumerator ShowNotificationCoroutine(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(notificationDisplayTime);

        // Dodatkowe sprawdzenie, czy tekst nadal powinien byÊ ukryty
        // (np. jeúli w miÍdzyczasie pojawi≥o siÍ nowe powiadomienie, to ono zarzπdza ukrywaniem)
        if (notificationText.text == message) // Ukryj tylko jeúli to nadal ten sam komunikat
        {
            notificationText.gameObject.SetActive(false);
        }
        _notificationCoroutine = null;
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
        }
    }

    private void TurnOffStationLight()
    {
        if (stationLight != null)
        {
            stationLight.enabled = false;
            stationLight.intensity = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stationCubeSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(stationCubeSpawnPoint.position, 0.25f);
            Gizmos.DrawLine(stationCubeSpawnPoint.position, stationCubeSpawnPoint.position + stationCubeSpawnPoint.forward * 0.5f);
        }
    }

    // ZNAJDè T  METOD 
    private IEnumerator PlayFirstChargeSubtitles()
    {
        // ZMIE— JEJ ZAWARTOå∆ NA PONIØSZ•:
        foreach (var line in firstChargeSubtitleLines)
        {
            // Uøywamy teraz nowej, globalnej metody
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(line);
            }
            // Nie musimy juø czekaÊ tutaj, menedøer sam zarzπdza czasem
        }
        // UsunÍliúmy yield return, poniewaø menedøer sam zarzπdza kolejkπ i opÛünieniami.
        // DziÍki temu napisy nie blokujπ dzia≥ania stacji ≥adowania.
        yield break; // ZakoÒcz korutynÍ od razu
    }
}