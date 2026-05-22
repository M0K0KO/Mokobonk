public enum VfxId : ushort
{
    None = 0,
    GunnerMuzzle = 1,
    GunnerBeam = 2,
    GunnerHitSpark = 3,
    MortarPredictionMarker = 4,
    MortarExplosion = 5,
    BuildableDestroyed = 6,
}

public enum VfxType : byte
{
    Point,   // ParticleSystem
    Beam,    // LineRenderer
    Marker,  // Position + Radius + Lifetime
}