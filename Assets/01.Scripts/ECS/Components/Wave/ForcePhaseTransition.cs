using Unity.Entities;

/// <summary>
/// ONLY USED IN EDITOR MODE
/// </summary>
public struct ForcePhaseTransition : IComponentData
{
    public GamePhase Target;
}