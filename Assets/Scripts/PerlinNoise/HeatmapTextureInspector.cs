using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(HeatmapTexture)), CanEditMultipleObjects]
public class HeatmapTextureInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HeatmapTexture heatmap = (HeatmapTexture)target;

        if (GUILayout.Button("Run"))
        {
            heatmap.Run();
        }
    }
}
#endif
