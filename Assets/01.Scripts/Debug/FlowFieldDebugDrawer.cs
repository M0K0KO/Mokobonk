using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldDebugDrawer : MonoBehaviour
{
    [SerializeField] private bool draw = true;
    [SerializeField] private Color arrowColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (!draw || !Application.isPlaying) return;

        var em = World.DefaultGameObjectInjectionWorld?.EntityManager;
        if (em == null) return;

        var fieldQuery = em.Value.CreateEntityQuery(typeof(FlowFieldSingleton));
        var gridQuery = em.Value.CreateEntityQuery(typeof(GridConfigSingleton));
        if (fieldQuery.CalculateEntityCount() == 0 || gridQuery.CalculateEntityCount() == 0) return;

        var field = fieldQuery.GetSingleton<FlowFieldSingleton>();
        var grid = gridQuery.GetSingleton<GridConfigSingleton>();

        Gizmos.color = arrowColor;
        for (int y = 0; y < field.Height; y++)
        {
            for (int x = 0; x < field.Width; x++)
            {
                var dir = field.Directions[x + y * field.Width];
                if (math.lengthsq(dir) < 1e-4f) continue;

                var cellCenter = (Vector3)GridUtility.CellToWorld(new int2(x, y), grid.Origin, grid.CellSize);
                Gizmos.DrawLine(cellCenter, cellCenter + (Vector3)dir * 0.4f);
            }
        }
    }
}