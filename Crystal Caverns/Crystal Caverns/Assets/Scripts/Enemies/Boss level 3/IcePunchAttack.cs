using System.Collections;
using UnityEngine;

public class IcePunchAttack : AttackBehaviour
{
    public IcePunchAttack(IceGolemBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        return distance <= ((IceGolemBossController)boss).punchRange;
    }

    public override void Execute()
    {
        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocityY);
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        return CanExecute() ? 12f : 0f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;

        // Wind-up animation
        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 5); 
        }

        // Face the player
        Vector2 direction = boss.GetDirectionToPlayer();
        boss.HandleFlip(direction.x);

        yield return new WaitForSeconds(0.4f); 

        // Execute punch
        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 5);
        }

        // Play punch sound
        if (!string.IsNullOrEmpty(boss.attackSoundName))
        {
            AudioManager.Instance?.PlaySFX(boss.attackSoundName);
        }

        yield return new WaitForSeconds(0.2f);

        // Check if player is still in range and deal damage
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        if (distance <= ((IceGolemBossController)boss).punchRange)
        {
            Hero2 playerController = boss.player.GetComponent<Hero2>();
            if (playerController != null)
            {
                playerController.TakeDamage(((IceGolemBossController)boss).punchDamage);

                ApplySlowEffect(playerController);
            }

            CreateIceImpactEffect();

            Debug.Log("Ice punch hit player!");
        }

        yield return new WaitForSeconds(0.5f); 

        // Return to idle animation
        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 0);
        }

        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }

    private void ApplySlowEffect(Hero2 playerController)
    {
        Debug.Log("Player slowed by ice punch!");
    }

    private void CreateIceImpactEffect()
    {
        Vector3 impactPosition = boss.player.position;

        GameObject effect = new GameObject("IceImpact");
        effect.transform.position = impactPosition;

        Object.Destroy(effect, 1f);
    }
}