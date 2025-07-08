using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;



public class ItemWorld : MonoBehaviour
{
    [SerializeField] private Item item;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetItem(Item item)
    {
        this.item = item;
        spriteRenderer.sprite = item.GetSprite();
    }

    public Item GetItem()
    {
        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public static void SpawnItemWorld(Vector2 position, Item item, int amount)
    {
        Transform itemWorldTransform = Instantiate(ItemAssets.Instance.pfItemWorld, position, Quaternion.identity);
        ItemWorld itemWorld = itemWorldTransform.GetComponent<ItemWorld>();
        itemWorld.SetItem(item);
        itemWorld.GetItem().amount = amount;


        Rigidbody2D rb = itemWorld.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float forceX = UnityEngine.Random.Range(-2f, 2f);
            float forceY = UnityEngine.Random.Range(3f, 5f);
            rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
        }

        Vector3 itemScale = itemWorldTransform.localScale;
        itemWorldTransform.localScale = Vector3.zero;
        itemWorldTransform.DOScale(itemScale, 0.4f).SetEase(Ease.OutBack);
    }

    private void SetLight2D(Item item)
    {
        switch (item.itemType)
        {
        }
    }
}