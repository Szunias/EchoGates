using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EnergyController : MonoBehaviour
{
    [Range(0, 100)] public float Percent = 100f;
    public float MaxEnergy = 100f;

    [Header("UI Reference")]
    [SerializeField] private Slider energyBar;

    public void AddEnergy(float amount)
    {
        Percent = Mathf.Min(Percent + amount, MaxEnergy);
        UpdateEnergyBar();
    }

    public bool ConsumeEnergy(float amount)
    {
        if (Percent >= amount)
        {
            Percent -= amount;
            UpdateEnergyBar();
            return true;
        }
        return false;
    }

    public bool HasEnergy(float amount)
    {
        return Percent >= amount;
    }

    private void Start()
    {
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        if (energyBar != null)
        {
            energyBar.maxValue = MaxEnergy;
            energyBar.value = Percent;
        }
    }
}
