using Unity.Entities;

public struct VATClipBlob
{
    public int StartFrame;
    public int FrameCount;
    public float Duration;
    public bool HasRootMotion;
    public int RootMotionOffset;
}

public struct VATBlob
{
    public BlobArray<VATClipBlob> Clips;
    public BlobArray<UnityEngine.Vector3> RootDeltas;
    public int TotalFrameCount;
    public float FPS;
}