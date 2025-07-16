using System.Collections;
using UnityEngine;

public class ShadowStrikeAttack : AttackBehaviour
{
    public ShadowStrikeAttack(ShadowBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        return distance <= ((ShadowBossController)boss).shadowStrikeRange;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        return CanExecute() ? 12f : 0f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 5);
        }

        yield return new WaitForSeconds(0.4f);

        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        if (distance <= ((ShadowBossController)boss).shadowStrikeRange)
        {
            Hero2 playerController = boss.player.GetComponent<Hero2>();
            if (playerController != null)
            {
                playerController.TakeDamage(((ShadowBossController)boss).shadowStrikeDamage);
            }

            AudioManager.Instance?.PlaySFX("Shadow_Strike");

            Debug.Log("Shadow strike hit player!");
        }

        yield return new WaitForSeconds(0.8f);

        boss.animator.SetInteger("state", 1);
        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}