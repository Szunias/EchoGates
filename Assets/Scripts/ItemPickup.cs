using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    public void Pickup()
    {
        InventoryManager.Instance.Add(item);
        Destroy(gameObject);

    }
   
}   
