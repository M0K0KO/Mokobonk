using Unity.Entities;

public enum GamePhase : byte
{
    Preparation,
    Horde,
}

public struct GamePhaseSingleton : IComponentData
{
    public GamePhase Phase;
    public float PhaseElapsed;
}
