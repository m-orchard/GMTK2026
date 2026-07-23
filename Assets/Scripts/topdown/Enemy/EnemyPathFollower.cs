using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyPathFollower : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float waypointArrivalThreshold = 0.15f;

    [Tooltip("Re-plan the path only when the goal moves further than this from the goal the current path was planned for.")]
    [SerializeField, Min(0.01f)] private float replanGoalTolerance = 0.5f;

    private EnemyMovement movement;
    private GridPathfinder pathfinder;

    private readonly List<Vector3> waypoints = new List<Vector3>();
    private int waypointIndex;
    private Vector3 plannedGoal;
    private bool hasPlan;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    public void MoveTowards(Vector3 goal)
    {
        NavigationGrid grid = NavigationGrid.Instance;
        if (grid == null)
        {
            movement.MoveTowards(goal);
            return;
        }

        pathfinder ??= new GridPathfinder(grid);

        if (!hasPlan || Vector3.Distance(goal, plannedGoal) > replanGoalTolerance)
        {
            PlanPathTo(goal);
        }

        FollowPath(goal);
    }

    private void PlanPathTo(Vector3 goal)
    {
        plannedGoal = goal;
        waypointIndex = 0;
        hasPlan = pathfinder.TryFindPath(transform.position, goal, waypoints);
    }

    private void FollowPath(Vector3 goal)
    {
        if (!hasPlan || waypoints.Count == 0)
        {
            movement.MoveTowards(goal);
            return;
        }

        while (waypointIndex < waypoints.Count - 1
            && movement.DistanceTo(waypoints[waypointIndex]) <= waypointArrivalThreshold)
        {
            waypointIndex++;
        }

        movement.MoveTowards(waypoints[waypointIndex]);
    }
}
