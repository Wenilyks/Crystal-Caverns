using UnityEngine;

public abstract class EnemyState
{
    public EnemyController enemy;
    public EnemyState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract void OnDrawGizmos();
}