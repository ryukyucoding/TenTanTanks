using System.Collections.Generic;
using UnityEngine;

public enum PivotType
{
    RandomTurn = 2,
    NavTurn = 1,
}

public class AIMovementController : MonoBehaviour
{
    [SerializeField] private EnemyTank tank;

    public bool DoMovements = true;
    public bool DoMoveTowards = true;

    public int CurrentRandomMove;

    public readonly Queue<(Vector3 Direction, PivotType Type)> PivotQueue = new Queue<(Vector3, PivotType)>();
    public readonly Queue<Vector3> SubPivotQueue = new Queue<Vector3>();

    public bool IsSurviving;
    public bool IsTooCloseToObstacle;

    public void TickMovement()
    {
        if (tank == null || !DoMovements) return;

        if (IsSurviving)
            DoBlockNav();

        TryGenerateSubQueue();
        TryWorkSubQueue();
    }

    public void DoBlockNav()
    {
        if (IsSurviving) return;
        float checkDist = Mathf.Max(0.5f, tank.ObstacleAwarenessMovement() / 2f);
        IsTooCloseToObstacle = RaycastAheadOfTank(checkDist);
        if (!IsTooCloseToObstacle) return;

        float angleDiff = Mathf.PI * 0.25f * 0.5f;
        float fracL = -1f;
        float fracR = -1f;

        bool left = RaycastAheadOfTank(checkDist * 100f, -angleDiff, (f) => fracL = f);
        bool right = RaycastAheadOfTank(checkDist * 100f, angleDiff, (f) => fracR = f);

        var goBack = Mathf.Abs(fracL - fracR) <= 0.00125f;
        float vecRot;
        float redirectAngle = Mathf.PI * 0.5f;
        if (!goBack)
            vecRot = (fracL > fracR ? -redirectAngle : redirectAngle);
        else
            vecRot = Mathf.PI + Random.Range(-0.5f, 0.5f);

        var movementDirection = Quaternion.Euler(0, vecRot * Mathf.Rad2Deg, 0) * Vector3.forward;
        if (!HasNavTurn())
            PivotQueue.Enqueue((movementDirection, PivotType.NavTurn));
    }

    private bool HasNavTurn()
    {
        foreach (var p in PivotQueue)
            if (p.Type == PivotType.NavTurn) return true;
        return false;
    }

    public bool RaycastAheadOfTank(float distance, float offset = 0f, System.Action<float> hitFrac = null)
    {
        bool blocked = false;
        var dir = Quaternion.Euler(0, (tank.ChassisRotationDeg() + offset * Mathf.Rad2Deg), 0) * Vector3.forward;
        var start = tank.transform.position + Vector3.up * 0.25f;
        var end = start + dir * distance;
        if (Physics.Raycast(start, dir, out var hit, distance, tank.ObstacleMask()))
        {
            blocked = true;
            hitFrac?.Invoke(hit.distance / distance);
        }
        return blocked;
    }

    public bool TryGenerateSubQueue()
    {
        if (PivotQueue.Count == 0) return false;
        if (SubPivotQueue.Count > 0) return false;
        var pivot = PivotQueue.Dequeue();
        int cuts = Mathf.Max(1, tank.MaxQueuedMovements());
        var start = Quaternion.Euler(0, tank.ChassisRotationDeg(), 0) * Vector3.forward;
        for (int i = 0; i < cuts; i++)
        {
            var t = 1f / cuts * (i + 1);
            var dir = Vector3.Slerp(start, pivot.Direction, t);
            SubPivotQueue.Enqueue(dir);
        }
        return true;
    }

    public bool TryWorkSubQueue()
    {
        if (SubPivotQueue.Count == 0) return false;
        var dir = SubPivotQueue.Dequeue();
        tank.SetDesiredChassisRotation(dir);
        return true;
    }
}


