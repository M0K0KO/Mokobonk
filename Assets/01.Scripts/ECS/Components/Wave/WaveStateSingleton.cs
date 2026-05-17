using Unity.Entities;


public enum WavePhase : byte
{
    Preparing,
    Spawning,
    Clearing,
}

public struct WaveStateSingleton : IComponentData
{
    public int CurrentWave;
    public int AliveEnemies;
    public float NextWaveTime;
    public WavePhase Phase;
}