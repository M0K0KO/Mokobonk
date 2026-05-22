using Unity.Entities;

public struct BalanceMultiplierSingleton : IComponentData
{
    public float EnemyHpMul;
    public float EnemySpeedMul;
    public float EnemyDamageMul;
    public float SpawnRateMul;
    public float WalkerRatio; 
    public float TurretDamageMul;
    public float TurretFireRateMul;
    public float TurretRangeMul;
}