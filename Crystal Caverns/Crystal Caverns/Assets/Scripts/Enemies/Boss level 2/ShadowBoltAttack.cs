using System.Collections;
using UnityEngine;

public class ShadowBoltAttack : AttackBehaviour
{
    public ShadowBoltAttack(ShadowBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        ShadowBossController shadowBoss = (ShadowBossController)boss;
        return shadowBoss.shadowBoltPrefab != null &&
               shadowBoss.shadowBoltSpawnPoints != null &&
               shadowBoss.shadowBoltSpawnPoints.Length > 0;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        if (distance > ((ShadowBossController)boss).shadowStrikeRange && distance <= boss.attackRange)
            return 8f;
        return 4f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;
        ShadowBossController shadowBoss = (ShadowBossController)boss;

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 6);
        }

        yield return new WaitForSeconds(0.6f);

        Transform spawnPoint = shadowBoss.shadowBoltSpawnPoints[0];
        if (spawnPoint != null)
        {
            GameObject shadowBolt = Object.Instantiate(shadowBoss.shadowBoltPrefab, spawnPoint.position, Quaternion.identity);
            shadowBolt.GetComponentInChildren<SpriteRenderer>().color = new Color(0.5f, 0f, 0.8f, 1f); // Purple tint

            Vector2 direction = (boss.player.position - spawnPoint.position).normalized;

            ShadowBolt boltScript = shadowBolt.GetComponent<ShadowBolt>();
            if (boltScript == null)
            {
                boltScript = shadowBolt.AddComponent<ShadowBolt>();
                boltScript.GetComponentInChildren<Animator>().SetTrigger("midAttack");
            }

            boltScript.Initialize(10f, direction, shadowBoss.shadowBoltDamage);

            AudioManager.Instance?.PlaySFX("Shadow_Bolt");
        }

        yield return new WaitForSeconds(0.5f);

        boss.animator.SetInteger("state", 1);
        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}