using Unity.Mathematics;
using UnityEngine;

public static class GridUtility
{
    public static int2 WorldToCell(float3 world, float3 origin, float cellSize)
    {
        float3 local = world - origin;
        return new int2(
                (int)math.floor(local.x / cellSize),
                (int)math.floor(local.z / cellSize)
            );
    }

    public static float3 CellToWorld(int2 cell, float3 origin, float cellSize)
    {
        return origin + new float3(
                (cell.x + 0.5f) * cellSize,
                0f,
                (cell.y + 0.5f) * cellSize
            );
    }

    public static bool IsInBounds(int2 cell, int2 gridSize)
    {
        return cell.x >= 0 && cell.x < gridSize.x
            && cell.y >= 0 && cell.y < gridSize.y;
    }
}
