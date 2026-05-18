using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MokoVATBaker.Editor
{
    public class VATBakerWindow : EditorWindow
    {
        [System.Serializable]
        private class ClipEntry
        {
            public AnimationClip clip;
            public bool extractRootMotion;
        }

        private SkinnedMeshRenderer sourceSMR;
        private List<ClipEntry> clipEntries = new() { new ClipEntry() };
        private float fps = 30f;
        private string outputFolder = "Assets/VAT";
        private string assetName = "Zombie";

        [MenuItem("Tools/VAT Baker")]
        private static void Open() => GetWindow<VATBakerWindow>("VAT Baker");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("VAT Baker", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            sourceSMR = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(
                "Source SkinnedMeshRenderer", sourceSMR, typeof(SkinnedMeshRenderer), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);

            for (int i = 0; i < clipEntries.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                clipEntries[i].clip = (AnimationClip)EditorGUILayout.ObjectField(
                    clipEntries[i].clip, typeof(AnimationClip), false);
                clipEntries[i].extractRootMotion = EditorGUILayout.ToggleLeft(
                    "RootMotion", clipEntries[i].extractRootMotion, GUILayout.Width(100));
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    clipEntries.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Clip"))
                clipEntries.Add(new ClipEntry());

            EditorGUILayout.Space();
            fps = EditorGUILayout.FloatField("FPS", fps);
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            assetName = EditorGUILayout.TextField("Asset Name", assetName);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!CanBake()))
            {
                if (GUILayout.Button("Bake", GUILayout.Height(30)))
                {
                    var validClips = clipEntries.FindAll(c => c.clip != null);
                    var clips = validClips.ConvertAll(c => c.clip).ToArray();
                    var roots = validClips.ConvertAll(c => c.extractRootMotion).ToArray();

                    VATBaker.Bake(sourceSMR, clips, roots, fps, outputFolder, assetName);
                }
            }
        }

        private bool CanBake()
        {
            if (sourceSMR == null) return false;
            if (fps <= 0) return false;
            foreach (var e in clipEntries)
                if (e.clip != null) return true;
            return false;
        }
    }
}