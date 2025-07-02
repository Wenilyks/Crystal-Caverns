using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FlyingDeathState : FlyingEnemyState
{
    public FlyingDeathState(FlyingEnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.animator.SetInteger("state", 5);
    }

    public override void Update()
    {
        enemy.rb.linearVelocity = Vector2.Lerp(enemy.rb.linearVelocity, Vector2.down, Time.fixedDeltaTime * 10f);
    }
    public override void Exit()
    {
        
    }

    public override void OnDrawGizmos()
    {
        
    }
}
