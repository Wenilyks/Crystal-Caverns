using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class BossState
{
    protected FireWizardBossController boss;

    public BossState(FireWizardBossController boss)
    {
        this.boss = boss;  
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
