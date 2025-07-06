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

}
