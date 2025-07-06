using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public static ItemAssets Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Sprite healthPotionSprite;
    public Sprite strengthPotionSprite;
    public Sprite speedPotionSprite;
    public Sprite jumpPotionSprite;
    public Sprite manaPotionSprite;

    public Sprite healthRegenerationRingSprite;
    public Sprite manaRegenrationRingSprite;
    public Sprite speedRingSprite;
    public Sprite strengthRingSprite;
    public Sprite protectionRingSprite;

    public Sprite coinSprite;

}
