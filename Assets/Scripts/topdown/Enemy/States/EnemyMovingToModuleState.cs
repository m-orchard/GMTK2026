using UnityEngine;

public class EnemyMovingToModuleState : IState
{
    private readonly EnemyAI enemy;
    private ShipModule targetModule;

    public EnemyMovingToModuleState(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        targetModule = enemy.FindClosestModule();
    }

    public void Tick()
    {
        if (targetModule == null || targetModule.IsBroken)
        {
            enemy.TransitionTo(enemy.ReturningState);
            return;
        }

        Vector3 targetPosition = targetModule.transform.position;
        enemy.MoveTowards(targetPosition);

        if (enemy.IsWithinAttackRange(targetPosition))
        {
            enemy.AttackingModuleState.SetTarget(targetModule);
            enemy.TransitionTo(enemy.AttackingModuleState);
        }
    }

    public void Exit()
    {
        targetModule = null;
    }
}
