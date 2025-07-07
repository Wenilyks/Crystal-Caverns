using UnityEngine;

[System.Serializable]
public class Item
{
    public enum ItemType
    {
        Coins,
        HealthPotion,
        StrengthPotion,
        JumpPotion,
        SpeedPotion,
        ManaPotion,
        StrengthRing,
        ManaRing,
        HealthRing
    }

    public ItemType itemType;
    public int amount;

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
                return ItemAssets.Instance.healthPotionSprite;
            case ItemType.Coins:
                return ItemAssets.Instance.coinSprite;
            case ItemType.HealthPotion:
                return ItemAssets.Instance.healthPotionSprite; 
            case ItemType.StrengthPotion:
                return ItemAssets.Instance.strengthPotionSprite;
            case ItemType.JumpPotion:
                return ItemAssets.Instance.jumpPotionSprite;
            case ItemType.SpeedPotion:
                return ItemAssets.Instance.speedPotionSprite;
            case ItemType.ManaPotion:
                return ItemAssets.Instance.manaPotionSprite;
            case ItemType.StrengthRing:
                return ItemAssets.Instance.strengthRingSprite;
            case ItemType.ManaRing:
                return ItemAssets.Instance.manaRegenrationRingSprite;
            case ItemType.HealthRing:
                return ItemAssets.Instance.healthRegenerationRingSprite;

        }
    }

    public bool IsStackable()
    {
        switch (itemType)
        {
            default:
            case ItemType.Coins:
            case ItemType.HealthPotion:
            case ItemType.StrengthPotion:
            case ItemType.JumpPotion:
            case ItemType.SpeedPotion:
            case ItemType.ManaPotion:
                return true;
            case ItemType.StrengthRing:
            case ItemType.ManaRing:
            case ItemType.HealthRing:
                return false;
        }
    }
}
