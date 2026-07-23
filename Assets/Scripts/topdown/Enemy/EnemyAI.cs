using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(ResourceCarrier))]
public class EnemyAI : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField, Min(0f)] private float spawnDuration = 0.5f;

    [Header("Targeting")]
    [Tooltip("How close the enemy must get to a module before it starts attacking.")]
    [SerializeField, Min(0f)] private float attackRange = 0.75f;

    [Tooltip("How close the enemy must get to the return point to count as arrived.")]
    [SerializeField, Min(0.01f)] private float arrivalThreshold = 0.1f;

    [Header("Attacking")]
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackInterval = 0.5f;
    [SerializeField, Min(0f)] private float attackDuration = 3f;

    [Tooltip("Resources stolen from the module on each attack. Dropped if the enemy is killed, kept if it escapes.")]
    [SerializeField, Min(0)] private int resourcesPerAttack = 5;

    [Header("Retreat")]
    [Tooltip("Point the enemy returns to after attacking. Eventually the boarding point.")]
    [SerializeField] private Transform returnPoint;

    [Tooltip("How long the enemy waits at the return point before being destroyed.")]
    [SerializeField, Min(0f)] private float destroyDelayAfterReturn = 0.5f;

    private EnemyMovement movement;
    private ResourceCarrier resourceCarrier;
    private StateMachine stateMachine;

    public EnemySpawningState SpawningState { get; private set; }
    public EnemyMovingToModuleState MovingToModuleState { get; private set; }
    public EnemyAttackingModuleState AttackingModuleState { get; private set; }
    public EnemyReturningState ReturningState { get; private set; }

    public float SpawnDuration => spawnDuration;
    public float AttackDamage => attackDamage;
    public float AttackInterval => attackInterval;
    public float AttackDuration => attackDuration;
    public float DestroyDelayAfterReturn => destroyDelayAfterReturn;
    public Vector3 ReturnPosition => returnPoint != null ? returnPoint.position : transform.position;

    public event System.Action OnReachedReturnPoint;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        resourceCarrier = GetComponent<ResourceCarrier>();
        stateMachine = new StateMachine();

        SpawningState = new EnemySpawningState(this);
        MovingToModuleState = new EnemyMovingToModuleState(this);
        AttackingModuleState = new EnemyAttackingModuleState(this);
        ReturningState = new EnemyReturningState(this);
    }

    private void Start()
    {
        stateMachine.ChangeState(SpawningState);
    }

    private void Update()
    {
        stateMachine.Tick();
    }

    public void TransitionTo(IState nextState)
    {
        stateMachine.ChangeState(nextState);
    }

    public void MoveTowards(Vector3 target)
    {
        movement.MoveTowards(target);
    }

    public bool IsWithinAttackRange(Vector3 target)
    {
        return movement.DistanceTo(target) <= attackRange;
    }

    public bool HasArrivedAt(Vector3 target)
    {
        return movement.DistanceTo(target) <= arrivalThreshold;
    }

    public ShipModule FindClosestModule()
    {
        ShipModule[] modules = FindObjectsByType<ShipModule>(FindObjectsSortMode.None);
        ShipModule closest = null;
        float closestSqrDistance = float.MaxValue;

        foreach (ShipModule module in modules)
        {
            if (module.IsBroken)
            {
                continue;
            }

            float sqrDistance = (module.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = module;
            }
        }

        return closest;
    }

    public void CollectAttackResources()
    {
        resourceCarrier.Collect(resourcesPerAttack);
    }

    public void SetReturnPoint(Transform point)
    {
        returnPoint = point;
    }

    public void NotifyReachedReturnPoint()
    {
        OnReachedReturnPoint?.Invoke();
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
