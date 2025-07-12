using System.Collections;
using UnityEngine;

public class FireballAttack : AttackBehaviour 
{ 
    public FireballAttack(FireWizardBossController boss) : base(boss) { }

    public override bool CanExecute()
    {
        return boss.fireballPrefab != null && boss.firePoint != null;
    }

    public override void Execute()
    {
        boss.StartCoroutine(ExecuteAttack());
    }

    public override float GetPriority()
    {
        return CanExecute() ? 5f : 0f;
    }
    private IEnumerator ExecuteAttack()
    {
        boss.isAttacking = true;

        //if (boss.animator != null)
        //{
        //    boss.animator.SetTrigger("Fireball");
        //}

        yield return new WaitForSeconds(0.3f);

        GameObject fireball = Object.Instantiate(boss.fireballPrefab, boss.firePoint.position, Quaternion.identity);
        Vector2 direction = (boss.player.position - boss.firePoint.position).normalized;

        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript == null)
        {
            fireballScript = fireball.AddComponent<Fireball>();
        }

        fireballScript.Initialize(8f, direction.x, true, boss.player);

        yield return new WaitForSeconds(0.7f);

        boss.isAttacking = false;
        boss.lastAttackTime = Time.time;
    }
}
