using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(AgentSystem)), CanEditMultipleObjects]
public class AgentSystemInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AgentSystem agentSystem = (AgentSystem)target;

        GUILayout.Space(20);

        if (GUILayout.Button("Restore Terrain"))
        {
            agentSystem.RestoreTerrain();
        }
        if (GUILayout.Button("Execute"))
        {
            agentSystem.Execute();
        }
    }
}
#endif
