using Unity.Cinemachine.Editor;
using Unity.Entities;

public struct WaveConfigSingleton : IComponentData
{
    public float PrepareTime;
    public int BaseEnemyCount;
    public int EnemyCountPerWave;
    public float BaseSpawnInterval;
    public float MinSpawnInterval;
    public float IntervalDecayPerWave;
}