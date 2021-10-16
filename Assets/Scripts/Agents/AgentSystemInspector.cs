using System.Collections;
using System.Collections.Generic;
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

        GUILayout.Space(10);

        if (GUILayout.Button("Execute"))
        {
            agentSystem.Execute();
        }
    }
}
#endif
