using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> Items = new List<Item>();

    public Transform ItemContent;
    public GameObject InventoryItem;
    private void Awake()
    {
        Instance = this;        
    }
    public void Add(Item item)
    { 
        Items.Add(item);
    }

    public void Remove(Item item) 
    {
        Items.Remove(item);
    }

    public void ListItems()
    {
        // ZMIANA: przed wypisaniem itemów czyœcimy poprzednie entry w ItemContent
        foreach (Transform child in ItemContent)
            Destroy(child.gameObject);

        foreach (Item item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            var itemName = obj.transform.GetComponentInChildren<TMP_Text>();
            var itemIcon = obj.transform.GetComponentInChildren<Image>();

            itemName.text = item.itemName;
            itemIcon.sprite = item.icon;
        }
    }

}
