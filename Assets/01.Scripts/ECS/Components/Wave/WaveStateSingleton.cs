using Unity.Entities;


public enum WavePhase : byte
{
    Preparing,
    Spawning,
    Clearing,
}

public struct WaveStateSingleton : IComponentData
{
    public WavePhase Phase;
    public int CurrentWave;
    public float NextWaveTime;
    public int RemainingEnemies;
}