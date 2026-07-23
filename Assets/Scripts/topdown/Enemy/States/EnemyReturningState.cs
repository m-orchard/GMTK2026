using UnityEngine;

public class EnemyReturningState : IState
{
    private readonly EnemyAI enemy;
    private bool hasReturned;
    private float despawnTime;

    public EnemyReturningState(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        hasReturned = false;
    }

    public void Tick()
    {
        if (hasReturned)
        {
            if (Time.time >= despawnTime)
            {
                enemy.Despawn();
            }

            return;
        }

        enemy.MoveTowards(enemy.ReturnPosition);

        if (enemy.HasArrivedAt(enemy.ReturnPosition))
        {
            hasReturned = true;
            despawnTime = Time.time + enemy.DestroyDelayAfterReturn;
            enemy.NotifyReachedReturnPoint();
        }
    }

    public void Exit()
    {
    }
}
