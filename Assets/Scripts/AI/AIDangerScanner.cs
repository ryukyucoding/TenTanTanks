using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIDangerScanner : MonoBehaviour
{
    [SerializeField] private LayerMask projectileLayers;
    [SerializeField] private LayerMask mineLayers;
    [SerializeField] private float scanRadius = 20f;
    [SerializeField] private int selfTeam = 0;

    public readonly List<IAITankDanger> NearbyDangers = new List<IAITankDanger>();

    public void Scan(float awarenessFriendlyShell, float awarenessHostileShell, float awarenessFriendlyMine, float awarenessHostileMine)
    {
        NearbyDangers.Clear();

        var pos = transform.position;

        // projectiles
        var hits = Physics.OverlapSphere(pos, scanRadius, projectileLayers);
        foreach (var hit in hits)
        {
            var danger = hit.GetComponent<IAITankDanger>();
            if (danger == null) continue;
            var hostile = danger.Team != selfTeam && danger.Team != -1;
            var maxDist = hostile ? awarenessHostileShell : awarenessFriendlyShell;
            if (Vector3.Distance(pos, danger.Position) <= maxDist)
                NearbyDangers.Add(danger);
        }

        // mines
        var mines = Physics.OverlapSphere(pos, scanRadius, mineLayers);
        foreach (var hit in mines)
        {
            var danger = hit.GetComponent<IAITankDanger>();
            if (danger == null) continue;
            var hostile = danger.Team != selfTeam && danger.Team != -1;
            var maxDist = hostile ? awarenessHostileMine : awarenessFriendlyMine;
            if (Vector3.Distance(pos, danger.Position) <= maxDist)
                NearbyDangers.Add(danger);
        }
    }

    public bool TryGetAverageDanger(out Vector3 average)
    {
        average = Vector3.zero;
        if (NearbyDangers.Count == 0) return false;
        average = NearbyDangers.Aggregate(Vector3.zero, (sum, d) => sum + d.Position) / NearbyDangers.Count;
        return true;
    }
}


