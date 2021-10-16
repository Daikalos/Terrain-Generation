using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSystem : MonoBehaviour
{
    [SerializeField]
    private TerrainMesh _Terrain;
    [SerializeField, Min(0)]
    private uint _FlattenAgentTokens;
    [SerializeField, Min(0)]
    private uint _BeachAgentTokens;
    [SerializeField, Min(0)]
    private uint _RiverAgentTokens;
    [SerializeField, Min(0)]
    private uint _ErosionAgentTokens;

    private float[] _HeightMap;

    private Graph _Graph;

    private void OnValidate()
    {

    }

    public void Execute()
    {
        _Graph = new Graph(_Terrain.Width, _Terrain.Height);
        _Graph.SetVertices(_Terrain.Mesh.vertices);

        ModifyTerrain();
    }

    private void ModifyTerrain()
    {
        Vector3[] vertices = _Terrain.Mesh.vertices;
        for (int x = 0; x < _Terrain.Width; ++x)
        {
            for (int z = 0; z < _Terrain.Height; ++z)
            {
                int i = x + z * _Terrain.Width;
                vertices[i].y = _HeightMap[i];
            }
        }
        _Terrain.Mesh.vertices = vertices;
    }
}
