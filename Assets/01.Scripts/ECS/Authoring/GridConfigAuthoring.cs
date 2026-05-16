using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridConfigAuthoring : MonoBehaviour
{
    [Header("Grid Config")]
    public float CellSize = 1f;
    public Vector2Int GridSize = new Vector2Int(100, 100);
    public Vector3 Origin = new Vector3(-50f, 0f, -50f);

    [Header("Gizmo")]
    [SerializeField] private bool drawGrid = true;
    [SerializeField] private bool drawBounds = true;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color boundsColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private Color originColor = Color.red;
    [SerializeField] private Color centerColor = Color.cyan;

    private class Baker : Baker<GridConfigAuthoring>
    {
        public override void Bake(GridConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GridConfigSingleton
            {
                CellSize = authoring.CellSize,
                GridSize = new int2(authoring.GridSize.x, authoring.GridSize.y),
                Origin = authoring.Origin
            });
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGrid) DrawGridLines();
        if (drawBounds) DrawBounds();
    }

    private void DrawGridLines()
    {
        Gizmos.color = gridColor;
        float w = GridSize.x * CellSize;
        float h = GridSize.y * CellSize;

        for (int x = 0; x <= GridSize.x; x++)
        {
            Vector3 a = Origin + new Vector3(x * CellSize, 0f, 0f);
            Vector3 b = Origin + new Vector3(x * CellSize, 0f, h);
            Gizmos.DrawLine(a, b);
        }
        for (int z = 0; z <= GridSize.y; z++)
        {
            Vector3 a = Origin + new Vector3(0f, 0f, z * CellSize);
            Vector3 b = Origin + new Vector3(w, 0f, z * CellSize);
            Gizmos.DrawLine(a, b);
        }
    }

    private void DrawBounds()
    {
        Gizmos.color = boundsColor;
        float w = GridSize.x * CellSize;
        float h = GridSize.y * CellSize;
        Vector3 c = Origin + new Vector3(w * 0.5f, 0f, h * 0.5f);
        Gizmos.DrawWireCube(c, new Vector3(w, 0.01f, h));
    }
}
