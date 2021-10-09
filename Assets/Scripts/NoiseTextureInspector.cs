using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(NoiseTexture)), CanEditMultipleObjects]
public class NoiseTextureInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NoiseTexture texture = (NoiseTexture)target;

        if (GUILayout.Button("Create Texture"))
        {
            texture.CreateTexture();
        }
    }
}
#endif
