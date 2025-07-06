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
}