using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FireballRainAttack : AttackBehaviour
{
    public FireballRainAttack(FireWizardBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        return boss.fireballPrefab != null;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        float healthPercentage = boss.currentHealth / boss.maxHealth;

        if (healthPercentage < 0.3f) return 8f;
        if (healthPercentage < 0.7f) return 3f;

        return 1f;
    }
    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;

        //if (boss.animator != null)
        //{
        //    boss.animator.SetTrigger("FireballRain");
        //}

        yield return new WaitForSeconds(0.5f);

        int fireballCount = 5;
        float timeBetweenFireballs = 0.3f;

        for (int i = 0;  i < fireballCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                boss.player.position.x + UnityEngine.Random.Range(-3f, 3f),
                boss.player.position.y + 8f,
                0f
            );

            AudioManager.Instance.PlaySFX("Magic sphere");
            GameObject fireball = UnityEngine.Object.Instantiate(boss.fireballPrefab, spawnPosition, Quaternion.identity);

            fireball.transform.localRotation = Quaternion.Euler(0, 0, -90f);

            Debug.Log($"Fireball spawned at: {spawnPosition}");

            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            if (fireballRb != null)
            {
                Vector2 fallVelocity = new Vector2(
                    UnityEngine.Random.Range(-2f, 2f),
                    -8f * 0.7f
                );
                fireballRb.linearVelocity = fallVelocity;
            }

            UnityEngine.Object.Destroy(fireball, 4f);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;    
    }
}