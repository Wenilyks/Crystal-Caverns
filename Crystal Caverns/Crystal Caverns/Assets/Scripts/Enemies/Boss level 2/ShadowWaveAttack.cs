using System.Collections;
using UnityEngine;

public class ShadowWaveAttack : AttackBehaviour
{
    public ShadowWaveAttack(ShadowBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        return ((ShadowBossController)boss).shadowWavePrefab != null;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        float healthPercentage = boss.currentHealth / boss.maxHealth;

        if (healthPercentage < 0.3f) return 10f;
        if (healthPercentage < 0.6f) return 6f;

        return 3f;
    }

    private IEnumerator ExecuteAttack()
    {
        Debug.Log("Executing boss attack shadow bold");
        boss.isAttacking = true;
        ShadowBossController shadowBoss = (ShadowBossController)boss;

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 4);
        }

        yield return new WaitForSeconds(0.8f);

        int waveCount = 7;
        for (int i = 0; i < waveCount; i++)
        {
            float angle = (360f / waveCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3 spawnPos = boss.transform.position + (Vector3)(direction * 1.5f);
            GameObject shadowWave = Object.Instantiate(shadowBoss.shadowWavePrefab, spawnPos, Quaternion.identity);

            shadowWave.GetComponentInChildren<SpriteRenderer>().color = new Color(0.3f, 0f, 0.5f, 0.8f);

            ShadowWave waveScript = shadowWave.GetComponent<ShadowWave>();
            if (waveScript == null)
            {
                waveScript = shadowWave.AddComponent<ShadowWave>();
            }

            waveScript.Initialize(6f, direction, shadowBoss.shadowWaveDamage);

            yield return new WaitForSeconds(0.1f);
        }

        AudioManager.Instance?.PlaySFX("Shadow_Wave");

        yield return new WaitForSeconds(1.2f);

        boss.animator.SetInteger("state", 1);
        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}