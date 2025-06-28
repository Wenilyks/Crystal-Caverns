using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float explosionRadius = 1f;

    private bool hasExploded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        int layer = collision.gameObject.layer;
        if (((1 << layer) & enemyLayer) != 0 || ((1 << layer) & groundLayer) != 0)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("enemy is taking damage");
        }

        Destroy(gameObject);
    }
}