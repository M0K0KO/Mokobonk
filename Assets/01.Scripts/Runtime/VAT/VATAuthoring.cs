using MokoVATBaker;
using Unity.Entities;
using UnityEngine;

public class VATAuthoring : MonoBehaviour
{
    public VATAnimationSet animationSet;
    public int defaultClipIndex = 0;

    public class VATBaker : Baker<VATAuthoring>
    {
        public override void Bake(VATAuthoring authoring)
        {
            if (authoring.animationSet == null) return;

            var set = authoring.animationSet;
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            using var builder = new BlobBuilder(Unity.Collections.Allocator.Temp);
            ref var root = ref builder.ConstructRoot<VATBlob>();

            root.TotalFrameCount = set.totalFrameCount;
            root.FPS = set.fps;

            var clipsArr = builder.Allocate(ref root.Clips, set.clips.Length);
            for (int i =0;i < set.clips.Length; i++)
            {
                clipsArr[i] = new VATClipBlob
                {
                    StartFrame = set.clips[i].startFrame,
                    FrameCount = set.clips[i].frameCount,
                    Duration = set.clips[i].duration,
                    HasRootMotion = set.clips[i].hasRootMotion,
                    RootMotionOffset = set.clips[i].rootMotionOffset,
                };
            }

            var deltasArr = builder.Allocate(ref root.RootDeltas, set.rootDeltas.Length);
            for (int i =0; i < set.rootDeltas.Length; i++)
            {
                deltasArr[i] = set.rootDeltas[i];
            }

            var blobRef = builder.CreateBlobAssetReference<VATBlob>(Unity.Collections.Allocator.Persistent);
            AddBlobAsset(ref blobRef, out _);

            AddComponent(entity, new VATAsset { Blob = blobRef });

            int clipIdx = Mathf.Clamp(authoring.defaultClipIndex, 0, set.clips.Length - 1);
            var clip = set.clips[clipIdx];

            AddComponent(entity, new VATAnimationState
            {
                CurrentClipIndex = clipIdx,
                CurrentClipStartTime = 0f
            });

            AddComponent(entity, new VATClipStartFrame { Value = clip.startFrame });
            AddComponent(entity, new VATClipFrameCount { Value = clip.frameCount });
            AddComponent(entity, new VATClipStartTime { Value = 0f });
            AddComponent(entity, new VATPrevClipStartFrame { Value = clip.startFrame });
            AddComponent(entity, new VATPrevClipFrameCount { Value = clip.frameCount });
            AddComponent(entity, new VATPrevClipStartTime { Value = 0f });
            AddComponent(entity, new VATBlendFactor { Value = 1f });
        }
    }
}
