using Unity.Entities;

public enum GameState : byte { Playing, Won, Lost }

public struct GameStateSingleton : IComponentData
{
    public GameState State;
}