using UnityEngine;

public class EnemyItemDrop : MonoBehaviour
{
    [System.Serializable]
    public struct Loot
    {
        public GameObject pickupPrefab;          // prefab z�ItemPickup
        [Range(0f, 100f)]
        public float chancePercent;              // szansa w�%
    }

    [Header("Tabela �up�w (szanse w�%)")]
    public Loot[] lootTable = new Loot[3];       // domy�lnie 3�pozycje

    public void TryDrop()
    {
        foreach (var loot in lootTable)          // przechodzimy po�kolei
        {
            if (loot.pickupPrefab == null) continue;
            if (Random.value * 100f <= loot.chancePercent)
            {
                Instantiate(loot.pickupPrefab, transform.position, Quaternion.identity);
                break;                           // wypada TYLKO jeden przedmiot
            }
        }
    }
}
