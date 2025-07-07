using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Inventory
{
    public event Action OnItemListChanged;
    private List<Item> itemList;
    private float maxCapacity = 16f;
    public Inventory()
    {
        itemList = new List<Item>();
        Debug.Log("Inventory");
        Debug.Log(itemList.Count);
    }

    public void AddItem(Item item)
    {
        if (!CanAddItem(item)) return;
        if (item.IsStackable())
        {
            bool itemAlreadyInInventory = false;
            foreach (Item inventoryItem in itemList)
            {
                if (inventoryItem.itemType == item.itemType)
                {
                    inventoryItem.amount += item.amount;
                    itemAlreadyInInventory = true;
                }
            }

            if (!itemAlreadyInInventory)
            {
                itemList.Add(item);
            }
        }
        else
            itemList.Add(item);


        OnItemListChanged?.Invoke();
        Debug.Log($"Added new item: {item}");
    }

    public bool CanAddItem(Item item)
    {
        if (item.IsStackable())
        {
            if (itemList.Exists(i => i.itemType == item.itemType))
                return true;
        }
        return itemList.Count < maxCapacity;
    }

    public bool RemoveItem(Item.ItemType itemType, int amount = 1)
    {
        Item item = itemList.Find(i => i.itemType == itemType);
        if (item != null)
        {
            item.amount -= amount;
            if (item.amount <= 0)
                itemList.Remove(item);

            OnItemListChanged?.Invoke();
            return true;
        }
        return false;
    }

    public List<Item> GetItems()
    {
        return itemList;
    }
}