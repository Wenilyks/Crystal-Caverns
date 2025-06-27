using UnityEngine;

[System.Serializable]
public abstract class FlyingEnemyState
{
    public FlyingEnemyController enemy;

    public FlyingEnemyState(FlyingEnemyController enemy)
    {
        this.enemy = enemy;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract void OnDrawGizmos();

}
