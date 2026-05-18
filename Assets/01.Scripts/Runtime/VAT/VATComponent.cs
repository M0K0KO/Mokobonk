using Unity.Entities;
using Unity.Rendering;

public struct VATAsset : IComponentData
{
    public BlobAssetReference<VATBlob> Blob;
}

public struct VATAnimationState : IComponentData
{
    public int CurrentClipIndex;
    public float CurrentClipStartTime;
}

public struct VATAnimationTransition : IComponentData
{
    public int PrevClipIndex;
    public float PrevClipStartTime;
    public float TransitionStartTime;
    public float TransitionDuration;
}

[MaterialProperty("_ClipStartFrame")]
public struct VATClipStartFrame : IComponentData { public float Value; }


[MaterialProperty("_ClipFrameCount")]
public struct VATClipFrameCount : IComponentData { public float Value; }


[MaterialProperty("_ClipStartTime")]
public struct VATClipStartTime : IComponentData { public float Value; }


[MaterialProperty("_PrevClipStartFrame")]
public struct VATPrevClipStartFrame : IComponentData { public float Value; }


[MaterialProperty("_PrevClipFrameCount")]
public struct VATPrevClipFrameCount : IComponentData { public float Value; }


[MaterialProperty("_PrevClipStartTime")]
public struct VATPrevClipStartTime : IComponentData { public float Value; }


[MaterialProperty("_BlendFactor")]
public struct VATBlendFactor : IComponentData { public float Value; }