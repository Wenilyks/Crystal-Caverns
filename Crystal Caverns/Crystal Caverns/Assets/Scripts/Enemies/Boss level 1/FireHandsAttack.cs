using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FireHandsAttack : AttackBehaviour
{
    public FireHandsAttack(FireWizardBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        return distance <= boss.fireHandsRange;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        return CanExecute() ? 10f : 0f;
    }

    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;

        if (boss.animator != null)
        {
            boss.animator.SetInteger("state", 5);
        }

        yield return new WaitForSeconds(0.5f);

        float distance = Vector2.Distance(boss.transform.position, boss.player.position);
        
        if (distance <= boss.fireHandsRange)
        {
            Hero2 playerController = boss.player.GetComponent<Hero2>();

            if (playerController != null)
            {
                playerController.TakeDamage(boss.fireHandsDamage);
            }

            Debug.Log("Fire hands hit player!");
        }

        yield return new WaitForSeconds(1f);

        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}