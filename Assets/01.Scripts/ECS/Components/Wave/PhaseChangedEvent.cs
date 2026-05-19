using Unity.Entities;

public struct PhaseChangedEvent : IComponentData
{
    public GamePhase From;
    public GamePhase To;
    public int WaveIndex;
}