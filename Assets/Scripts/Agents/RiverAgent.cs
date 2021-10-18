using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RiverAgent : Agent
{
    private Vector2Int _StartPosition;
    private Vector2Int _GoalPosition;

    public RiverAgent(ref Graph graph, TerrainMesh terrain, AgentParams agentParams)
            : base(ref graph, terrain, agentParams) { }

    public override void Initialize()
    {
        _StartPosition = Terrain.Beaches[StaticRandom.Range(0, Terrain.Beaches.Count)];
        _GoalPosition = Terrain.Mountains[StaticRandom.Range(0, Terrain.Mountains.Count)];
    }

    public override bool Update(float deltaTime)
    {


        return true;
    }
}
