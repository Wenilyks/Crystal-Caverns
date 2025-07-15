using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AttackBehaviour
{
    protected BossController boss;

    public AttackBehaviour(BossController boss)
    {
        this.boss = boss;
    }

    public abstract bool CanExecute();
    public abstract void Execute();
    public abstract float GetPriority();
    public virtual string GetAttackName() => GetType().Name;
}