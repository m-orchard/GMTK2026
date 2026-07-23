using UnityEngine;

public class EnemySpawningState : IState
{
    private readonly EnemyAI enemy;
    private float spawnFinishTime;

    public EnemySpawningState(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        spawnFinishTime = Time.time + enemy.SpawnDuration;
    }

    public void Tick()
    {
        if (Time.time >= spawnFinishTime)
        {
            enemy.TransitionTo(enemy.MovingToModuleState);
        }
    }

    public void Exit()
    {
    }
}
