using Unity.Collections;
using Unity.Entities;
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

    public static bool AreAllCellsFree(
    int2 anchor, int2 size, int2 gridSize, NativeHashMap<int2, Entity> occupancy)
    {
        for (int dy = 0; dy < size.y; dy++)
            for (int dx = 0; dx < size.x; dx++)
            {
                int2 cell = new int2(anchor.x + dx, anchor.y + dy);
                if (!IsInBounds(cell, gridSize)) return false;
                if (occupancy.ContainsKey(cell)) return false;
            }
        return true;
    }

    public static float3 FootprintCenterWorld(
        int2 anchor, int2 size, float3 origin, float cellSize)
    {
        return origin + new float3(
            (anchor.x + size.x * 0.5f) * cellSize,
            0f,
            (anchor.y + size.y * 0.5f) * cellSize);
    }
}
