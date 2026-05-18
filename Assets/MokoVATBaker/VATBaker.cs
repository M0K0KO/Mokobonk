using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MokoVATBaker.Editor
{
    public static class VATBaker
    {
        public static void Bake(
            SkinnedMeshRenderer smr,
            AnimationClip[] clips, 
            bool[] extractRootMotion, 
            float fps, 
            string outputFolder, 
            string assetName)
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                AssetDatabase.Refresh();
            }

            var animator = smr.GetComponentInParent<Animator>();
            if (animator == null)
            {
                Debug.LogError("[VATBaker] Animator not found in parents.");
                return;
            }
            var source = animator.gameObject;

            var rootBone = smr.rootBone != null ? smr.rootBone : FindHips(source);
            if (rootBone == null)
            {
                Debug.LogError("[VATBaker] rootBone(Hips) not found.");
                return;
            }

            const int TEXELS_PER_ROW = 256;

            int vertexCount = smr.sharedMesh.vertexCount;
            int rowsPerFrame = Mathf.CeilToInt(vertexCount / (float)TEXELS_PER_ROW);

            var clipInfos = new VATClipInfo[clips.Length];
            int totalFrameCount = 0;
            int rootDeltaOffset = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                int frameCount = Mathf.Max(1, Mathf.CeilToInt(clips[i].length * fps));
                clipInfos[i] = new VATClipInfo
                {
                    name = clips[i].name,
                    startFrame = totalFrameCount,
                    frameCount = frameCount,
                    duration = clips[i].length,
                    hasRootMotion = extractRootMotion[i],
                    rootMotionOffset = extractRootMotion[i] ? rootDeltaOffset : -1
                };
                totalFrameCount += frameCount;
                if (extractRootMotion[i]) rootDeltaOffset += frameCount;
            }

            int texWidth = TEXELS_PER_ROW;
            int texHeight = rowsPerFrame * totalFrameCount;

            if (texHeight > 16384)
            {
                Debug.LogError($"[VATBaker] Texture height {texHeight} > 16384. " +
                               $"Reduce frames or increase TEXELS_PER_ROW.");
                return;
            }

            var posTex = new Texture2D(texWidth, texHeight,
                TextureFormat.RGBAFloat, false, true) { filterMode = FilterMode.Point };
            var nrmTex = new Texture2D(texWidth, texHeight,
                TextureFormat.RGBAFloat, false, true) { filterMode = FilterMode.Point };

            var rootDeltas = new List<Vector3>();

            AnimationMode.StartAnimationMode();
            try
            {
                var bakeMesh = new Mesh();
                var posBuffer = new Vector3[vertexCount];
                var nrmBuffer = new Vector3[vertexCount];

                for (int ci = 0; ci < clips.Length; ci++)
                {
                    var clip = clips[ci];
                    var info = clipInfos[ci];

                    Vector3 hipsBaseline = Vector3.zero;
                    if (info.hasRootMotion)
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(source, clip, 0f);
                        AnimationMode.EndSampling();
                        hipsBaseline = rootBone.localPosition;
                    }

                    for (int f = 0; f < info.frameCount; f++)
                    {
                        float t = (info.frameCount == 1) ? 0f : (f / (float)(info.frameCount - 1)) * clip.length;

                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(source, clip, t);
                        AnimationMode.EndSampling();

                        smr.BakeMesh(bakeMesh, true);

                        var posList = new List<Vector3>(vertexCount);
                        var nrmList = new List<Vector3>(vertexCount);
                        bakeMesh.GetVertices(posList);
                        bakeMesh.GetNormals(nrmList);

                        int row = info.startFrame + f;
                        for (int v = 0; v< vertexCount; v++)
                        {
                            int x = v % TEXELS_PER_ROW;
                            int localY = v / TEXELS_PER_ROW;
                            int y = (info.startFrame + f) * rowsPerFrame + localY;

                            var p = posList[v];
                            var n = nrmList[v];
                            posTex.SetPixel(x, y, new Color(p.x, p.y, p.z, 1f));
                            nrmTex.SetPixel(x, y, new Color(n.x, n.y, n.z, 0f));
                        }

                        if (info.hasRootMotion)
                        {
                            var delta = rootBone.localPosition - hipsBaseline;
                            rootDeltas.Add(delta);
                        }
                    }
                }

                Object.DestroyImmediate(bakeMesh);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }

            posTex.Apply(false, false);
            nrmTex.Apply(false, false);

            var bakedMesh = BuildBakedMesh(smr.sharedMesh, TEXELS_PER_ROW);

            string posPath = $"{outputFolder}/{assetName}_Position.exr";
            string nrmPath = $"{outputFolder}/{assetName}_Normal.exr";
            File.WriteAllBytes(posPath, posTex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
            File.WriteAllBytes(nrmPath, nrmTex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
            AssetDatabase.Refresh();

            ConfigureTextureImport(posPath);
            ConfigureTextureImport(nrmPath);

            var posAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(posPath);
            var nrmAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(nrmPath);

            string meshPath = $"{outputFolder}/{assetName}_Mesh.asset";
            AssetDatabase.CreateAsset(bakedMesh, meshPath);

            var set = ScriptableObject.CreateInstance<VATAnimationSet>();
            set.texelsPerRow = TEXELS_PER_ROW;
            set.rowsPerFrame = rowsPerFrame;
            set.positionTexture = posAsset;
            set.normalTexture = nrmAsset;
            set.bakedMesh = bakedMesh;
            set.vertexCount = vertexCount;
            set.totalFrameCount = totalFrameCount;
            set.fps = fps;
            set.clips = clipInfos;
            set.rootDeltas = rootDeltas.ToArray();

            string setPath = $"{outputFolder}/{assetName}.asset";
            AssetDatabase.CreateAsset(set, setPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[VATBaker] Done: {setPath}  ({vertexCount} verts ¡¿ {totalFrameCount} frames)");
            Selection.activeObject = set;
        }

        private static Transform FindHips(GameObject root)
        {
            var animator = root.GetComponentInChildren<Animator>();
            if (animator != null && animator.isHuman)
                return animator.GetBoneTransform(HumanBodyBones.Hips);
            return null;
        }

        private static Mesh BuildBakedMesh(Mesh source, int texelsPerRow)
        {
            var m = new Mesh();
            m.indexFormat = source.vertexCount > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            m.SetVertices(source.vertices);
            m.SetNormals(source.normals);
            m.SetTangents(source.tangents);
            m.SetUVs(0, new List<Vector2>(source.uv));

            int vc = source.vertexCount;
            var ids = new Vector2[vc];
            for (int i = 0; i < vc; i++)
            {
                float x = i % texelsPerRow + 0.5f;
                float localY = i / texelsPerRow + 0.5f;
                ids[i] = new Vector2(x, localY);
            }
            m.SetUVs(1, new List<Vector2>(ids));

            m.subMeshCount = source.subMeshCount;
            for (int s = 0; s < source.subMeshCount; s++)
                m.SetTriangles(source.GetTriangles(s), s);

            m.bounds = source.bounds; 
            return m;
        }

        private static void ConfigureTextureImport(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) return;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 16384;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}