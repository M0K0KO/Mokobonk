using Unity.Entities;
using UnityEngine;

public struct SpawnState : IComponentData
{
    public float Timer;
    public Unity.Mathematics.Random random;
    public int currentEnemyCount;
}
