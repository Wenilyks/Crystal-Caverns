using System.Collections;
using UnityEngine;

public class IceShardBarrageAttack : AttackBehaviour
{
    private float lastExecutionTime = 0f;
    private float attackCooldown = 3f; 
    public IceShardBarrageAttack(BossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        IceGolemBossController iceGolem = (IceGolemBossController)boss;

        if (iceGolem.iceShardPrefab == null ||
            iceGolem.leftSpawnPoint == null ||
            iceGolem.rightSpawnPoint == null)
        {
            return false;
        }

        if (boss.isAttacking)
        {
            return false;
        }

        if (Time.time - lastExecutionTime < attackCooldown)
        {
            return false;
        }

        if (boss.currentHealth <= 0)
        {
            return false;
        }

        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        return distance <= boss.attackRange;
    }

    public override void Execute()
    {
        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocityY);
        lastExecutionTime = Time.time;
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        if (!CanExecute())
        {
            return 0f;
        }

        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        float healthPercentage = boss.currentHealth / boss.maxHealth;
        IceGolemBossController iceGolem = (IceGolemBossController)boss;

        if (distance > iceGolem.punchRange && distance <= boss.attackRange)
        {
            float priority = 7f;

            if (healthPercentage < 0.5f) priority += 3f;
            if (healthPercentage < 0.25f) priority += 2f;

            return priority;
        }

        if (distance <= iceGolem.punchRange)
        {
            return 2f;
        }

        return 0f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;
        IceGolemBossController iceGolem = (IceGolemBossController)boss;

        Debug.Log("Starting Ice Shard Barrage Attack");

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 6); 
        }

        boss.rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.6f);

        float healthPercentage = boss.currentHealth / boss.maxHealth;
        int shardsPerSide = healthPercentage < 0.3f ? 4 : 3;

        for (int wave = 0; wave < shardsPerSide; wave++)
        {
            if (boss.currentHealth <= 0)
            {
                break;
            }

            SpawnIceShard(iceGolem.leftSpawnPoint, true);

            yield return new WaitForSeconds(0.15f);

            SpawnIceShard(iceGolem.rightSpawnPoint, false);

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.5f);

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 0);
        }

        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;

        Debug.Log("Ice Shard Barrage Attack completed");
    }

    private void SpawnIceShard(Transform spawnPoint, bool fromLeft)
    {
        IceGolemBossController iceGolem = (IceGolemBossController)boss;

        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y += Random.Range(-0.5f, 0.5f);

        GameObject iceShard = Object.Instantiate(iceGolem.iceShardPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer sr = iceShard.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
        }

        Vector2 directionToPlayer = (boss.player.position - spawnPos).normalized;
        float spreadAngle = Random.Range(-15f, 15f);
        directionToPlayer = RotateVector2(directionToPlayer, spreadAngle);

        Fireball iceShardScript = iceShard.GetComponent<Fireball>();
        if (iceShardScript == null)
        {
            iceShardScript = iceShard.AddComponent<Fireball>();
        }

        float speed = Random.Range(6f, 9f);
        iceShardScript.Initialize(speed, directionToPlayer.x, true, boss.player);

        if (!string.IsNullOrEmpty(iceGolem.iceShardSoundName))
        {
            AudioManager.Instance?.PlaySFX(iceGolem.iceShardSoundName);
        }

        Object.Destroy(iceShard, 4f);

        Debug.Log($"Ice shard spawned from {(fromLeft ? "left" : "right")} side");
    }

    private Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
}