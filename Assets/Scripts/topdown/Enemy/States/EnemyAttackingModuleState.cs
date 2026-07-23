using UnityEngine;

public class EnemyAttackingModuleState : IState {
    private readonly EnemyAI enemy;
    private ShipModule targetModule;
    private float attackFinishTime;
    private float nextAttackTime;

    public EnemyAttackingModuleState(EnemyAI enemy) {
        this.enemy = enemy;
    }

    public void SetTarget(ShipModule module) {
        targetModule = module;
    }

    public void Enter() {
        attackFinishTime = Time.time + enemy.AttackDuration;
        nextAttackTime = Time.time;
    }

    public void Tick() {
        if (ShouldStopAttacking()) {
            enemy.TransitionTo(enemy.ReturningState);
            return;
        }

        if (Time.time >= nextAttackTime) {
            targetModule.Health.TakeDamage(enemy.AttackDamage);
            enemy.CollectAttackResources();
            nextAttackTime = Time.time + enemy.AttackInterval;
            AudioManager.Instance.PlaySound(enemy.AttackSfx);
        }
    }

    private bool ShouldStopAttacking() {
        return targetModule == null
            || targetModule.IsBroken
            || Time.time >= attackFinishTime;
    }

    public void Exit() {
        targetModule = null;
    }
}