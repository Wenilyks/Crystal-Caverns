using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AttackBehaviour
{
    protected FireWizardBossController boss;

    public AttackBehaviour (FireWizardBossController boss)
    {
        this.boss = boss;
    }

    public abstract bool CanExecute();
    public abstract void Execute();
    public abstract float GetPriority();
}