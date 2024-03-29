using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainMesh)), CanEditMultipleObjects]
public class TerrainInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainMesh terrain = (TerrainMesh)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            terrain.Generate();
        }
    }
}
#endif
