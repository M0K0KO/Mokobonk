using UnityEngine;

namespace MokoVATBaker
{
    [System.Serializable]
    public struct VATClipInfo
    {
        public string name;
        public int startFrame;
        public int frameCount;
        public float duration;
        public bool hasRootMotion;
        public int rootMotionOffset;
    }

    [CreateAssetMenu(menuName = "MokoVATBaker/VAT Animation Set")]
    public class VATAnimationSet : ScriptableObject
    {
        public int texelsPerRow;
        public int rowsPerFrame;

        public Texture2D positionTexture;
        public Texture2D normalTexture;
        public Mesh bakedMesh;

        public int vertexCount;
        public int totalFrameCount;
        public float fps;

        public VATClipInfo[] clips;
        public Vector3[] rootDeltas;

        public int FindClipIndex(string name)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == name) return i;
            }

            return -1;
        }
    }
}