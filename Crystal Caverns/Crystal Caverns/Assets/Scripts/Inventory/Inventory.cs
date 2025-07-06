using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Inventory
{
    public event Action OnItemListChanged;
    private List<Item> itemList;
    public Inventory()
    {
        itemList = new List<Item>();
        Debug.Log("Inventory");

        AddItem(new Item() { itemType = Item.ItemType.JumpPotion, amount = 2 });
        AddItem(new Item() { itemType = Item.ItemType.SpeedPotion, amount = 1 });
        AddItem(new Item() { itemType = Item.ItemType.StrengthPotion, amount = 4});
        AddItem(new Item() { itemType = Item.ItemType.SpeedPotion, amount = 1 });
        AddItem(new Item() { itemType = Item.ItemType.StrengthPotion, amount = 4 }); 
        Debug.Log(itemList.Count);
    }

    public void AddItem(Item item)
    {
        itemList.Add(item);
        OnItemListChanged?.Invoke();
        Debug.Log($"Added new item: {item}");
    }

    public List<Item> GetItems()
    {
        return itemList;
    }
}