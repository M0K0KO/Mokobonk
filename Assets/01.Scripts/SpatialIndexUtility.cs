using Unity.Mathematics;

public static class SpatialIndexUtility
{
    public static int2 WorldToCell(float3 world, float3 origin, float cellSize)
    {
        float3 local = world - origin;
        return new int2(
                (int)math.floor(local.x / cellSize),
                (int)math.floor(local.z / cellSize)
            );
    }
}