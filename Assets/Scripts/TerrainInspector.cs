using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainMesh))]
public class TerrainInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainMesh terrain = (TerrainMesh)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Restore"))
        {
            terrain.Restore();
        }
    }
}
#endif
