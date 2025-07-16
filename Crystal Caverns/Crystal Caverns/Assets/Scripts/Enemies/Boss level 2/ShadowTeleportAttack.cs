using System.Collections;
using UnityEngine;

public class ShadowTeleportAttack : AttackBehaviour
{
    public ShadowTeleportAttack(ShadowBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        ShadowBossController shadowBoss = (ShadowBossController)boss;
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        return shadowBoss.CanTeleport() && distance > shadowBoss.teleportRange;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        return CanExecute() ? 7f : 0f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;
        ShadowBossController shadowBoss = (ShadowBossController)boss;

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 10);
        }

        if (shadowBoss.teleportEffect != null)
        {
            shadowBoss.teleportEffect.Play();
        }

        AudioManager.Instance?.PlaySFX("Shadow_Teleport");

        SpriteRenderer sprite = boss.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            float fadeTime = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Vector2 playerPos = boss.player.position;
        Vector2 teleportPos;

        Vector2 playerFacing = boss.player.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        teleportPos = playerPos - playerFacing * 3f;

        boss.transform.position = teleportPos;

        if (sprite != null)
        {
            Color originalColor = sprite.color;
            float fadeTime = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        shadowBoss.SetTeleportCooldown();

        yield return new WaitForSeconds(0.3f);

        boss.animator.SetInteger("state", 1);
        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}