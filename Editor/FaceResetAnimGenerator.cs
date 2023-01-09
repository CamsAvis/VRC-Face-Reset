using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

using VRC.SDK3.Avatars.Components;

public class FaceResetAnimGenerator : EditorWindow
{
    VRCAvatarDescriptor descriptor;
    List<string> blendshapeNames;
    Dictionary<string, bool> blendshapes;
    Vector2 scrollPos;

    [MenuItem("Cam/Face Reset Generator")]
    static void Open() => EditorWindow.GetWindow<FaceResetAnimGenerator>("Reset Animation Generator").Show();

    private void OnEnable()
    {
        blendshapeNames = new List<string>();
        blendshapes = new Dictionary<string, bool>();
        scrollPos = Vector2.zero;
        descriptor = null;
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Descriptor");
            EditorGUI.BeginChangeCheck();
            descriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(descriptor, typeof(VRCAvatarDescriptor), true);
            if (EditorGUI.EndChangeCheck()) {
                RefreshBlendshapes();
            }
        }

        if(GUILayout.Button("Generate")) {
            GenerateResetAnimation(descriptor);
        }

        GUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box");

        for(int i = 0; i < blendshapeNames.Count; i++) {
            string name = blendshapeNames[i];
            using(new EditorGUILayout.HorizontalScope()) {
                GUI.color = blendshapes[name] ? Color.white : Color.gray;
                blendshapes[name] = EditorGUILayout.Toggle(blendshapes[name], GUILayout.Width(25));
                GUILayout.Label(name);
                GUI.color = Color.white;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void RefreshBlendshapes()
    {
        blendshapeNames = new List<string>();
        blendshapes = new Dictionary<string, bool>();

        if (descriptor == null)
            return;

        SkinnedMeshRenderer smr = descriptor.VisemeSkinnedMesh;

        if (smr == null)
            return;

        for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++) {
            string blendshapeName = smr.sharedMesh.GetBlendShapeName(i);
            blendshapeNames.Add(blendshapeName);
            blendshapes.Add(blendshapeName, true);
        }
    }

    void GenerateResetAnimation(VRCAvatarDescriptor descriptor)
    {
        string savePath = EditorUtility.OpenFolderPanel("Create Animation", "Assets", string.Empty);

        AnimationClip clip = new AnimationClip();
        EditorUtility.SetDirty(clip);

        SkinnedMeshRenderer smr = descriptor.VisemeSkinnedMesh;

        foreach(KeyValuePair<string, bool> kvp in this.blendshapes) {
            if (kvp.Value == false)
                continue;

            string propertyName = "blendShape." + kvp.Key;
            string meshPath = smr.transform.GetHierarchyPath(descriptor.transform);
            Keyframe[] keys = new Keyframe[] { new Keyframe() { time = 0, value = 0 } };

            clip.SetCurve(
                meshPath, 
                typeof(SkinnedMeshRenderer), 
                propertyName, 
                new AnimationCurve() { keys = keys }
            );
        }

        savePath = savePath.Replace(Application.dataPath, "Assets");
        string saveName = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Reset.anim");
        AssetDatabase.CreateAsset(clip, saveName);
    }
}
