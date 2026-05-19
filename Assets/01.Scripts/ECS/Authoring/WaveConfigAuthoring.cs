using Unity.Entities;
using UnityEngine;

public class WaveConfigAuthoring : MonoBehaviour
{
    public float PrepareTime = 5f;
    public int BaseEnemyCount = 10;
    public int EnemyCountPerWave = 5;
    public float BaseSpawnInterval = 1.0f;
    public float MinSpawnInterval = 0.2f;
    public float IntervalDecayPerWave = 0.05f;

    private class Baker : Baker<WaveConfigAuthoring>
    {
        public override void Bake(WaveConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new WaveConfigSingleton
            {
                PrepareTime = authoring.PrepareTime,
                BaseEnemyCount = authoring.BaseEnemyCount,
                EnemyCountPerWave = authoring.EnemyCountPerWave,
                BaseSpawnInterval = authoring.BaseSpawnInterval,
                MinSpawnInterval = authoring.MinSpawnInterval,
                IntervalDecayPerWave = authoring.IntervalDecayPerWave
            });

            AddComponent(entity, new WaveStateSingleton
            {
                CurrentWave = 0,
                RemainingEnemies = 0,
                NextWaveTime = 0f,
                Phase = WavePhase.Preparing
            });
        }
    }
}