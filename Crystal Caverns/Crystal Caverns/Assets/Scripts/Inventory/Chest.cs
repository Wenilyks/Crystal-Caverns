using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private bool hasBeenOpened = false;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 3;

    [Header("Loot Tables")]
    [SerializeField] private LootItem[] commonLoot;
    [SerializeField] private LootItem[] rareLoot;
    [SerializeField] private LootItem[] epicLoot;

    [Header("Rarity Chances (0-100)")]
    [SerializeField] private float commonChance = 70f;
    [SerializeField] private float rareChance = 25f;
    [SerializeField] private float epicChance = 5f;

    [System.Serializable]
    public class LootItem
    {
        public Item.ItemType itemType;
        public int minAmount;
        public int maxAmount;
        public float dropChance = 100f;
    }

    private void Start()
    {
        if (commonLoot == null || commonLoot.Length == 0)
        {
            InitializeDefaultLootTables();
        }
    }

    private void InitializeDefaultLootTables()
    {
        commonLoot = new LootItem[]
        {
            new LootItem { itemType = Item.ItemType.Coins, minAmount = 5, maxAmount = 15, dropChance = 80f },
            new LootItem { itemType = Item.ItemType.HealthPotion, minAmount = 1, maxAmount = 3, dropChance = 60f },
            new LootItem { itemType = Item.ItemType.ManaPotion, minAmount = 1, maxAmount = 2, dropChance = 40f }
        };

        rareLoot = new LootItem[]
        {
            new LootItem { itemType = Item.ItemType.StrengthPotion, minAmount = 1, maxAmount = 2, dropChance = 70f },
            new LootItem { itemType = Item.ItemType.SpeedPotion, minAmount = 1, maxAmount = 2, dropChance = 70f },
            new LootItem { itemType = Item.ItemType.JumpPotion, minAmount = 1, maxAmount = 2, dropChance = 70f },
            new LootItem { itemType = Item.ItemType.Coins, minAmount = 20, maxAmount = 50, dropChance = 50f }
        };

        epicLoot = new LootItem[]
        {
            new LootItem { itemType = Item.ItemType.StrengthRing, minAmount = 1, maxAmount = 1, dropChance = 30f },
            new LootItem { itemType = Item.ItemType.ManaRing, minAmount = 1, maxAmount = 1, dropChance = 30f },
            new LootItem { itemType = Item.ItemType.HealthRing, minAmount = 1, maxAmount = 1, dropChance = 30f },
            new LootItem { itemType = Item.ItemType.Coins, minAmount = 100, maxAmount = 200, dropChance = 80f }
        };
    }

    public void OpenChest()
    {
        if (hasBeenOpened)
        {
            Debug.Log("no");
            return;
        }

        hasBeenOpened = true;
        GenerateRandomLoot();

        Debug.Log("Opened!");
    }

    private void GenerateRandomLoot()
    {
        int itemCount = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < itemCount; i++)
        {
            LootItem[] selectedLootTable = GetRandomLootTable();

            if (selectedLootTable.Length > 0)
            {
                LootItem randomLootItem = selectedLootTable[Random.Range(0, selectedLootTable.Length)];

                if (Random.Range(0f, 100f) <= randomLootItem.dropChance)
                {
                    int amount = Random.Range(randomLootItem.minAmount, randomLootItem.maxAmount + 1);

                    Item newItem = new Item
                    {
                        itemType = randomLootItem.itemType,
                        amount = amount
                    };

                    Vector3 spawnPos = spawnPosition.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
                    ItemWorld.SpawnItemWorld(spawnPos, newItem, amount);

                    Debug.Log($"Spawned {amount} {randomLootItem.itemType}");
                }
            }
        }
    }

    private LootItem[] GetRandomLootTable()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= epicChance)
        {
            Debug.Log("Epic loot");
            return epicLoot;
        }
        else if (randomValue <= epicChance + rareChance)
        {
            Debug.Log("Rare loot");
            return rareLoot;
        }
        else
        {
            Debug.Log("Common loot");
            return commonLoot;
        }
    }

    public void ResetChest()
    {
        hasBeenOpened = false;
        Debug.Log("Reset");
    }

    public bool IsOpened()
    {
        return hasBeenOpened;
    }
}