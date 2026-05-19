public enum VfxId : ushort
{
    None = 0,
    GunnerMuzzle = 1,
    GunnerBeam = 2,
    GunnerHitSpark = 3,
}

public enum VfxType : byte
{
    Point,   // ParticleSystem
    Beam,    // LineRenderer
}