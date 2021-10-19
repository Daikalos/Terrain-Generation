using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Agent
{
    private Graph _Graph;
    private TerrainMesh _Terrain;
    private AgentParams _AgentParams;

    protected int _CurrentTokens = 0;

    protected Graph Graph => _Graph;
    protected TerrainMesh Terrain => _Terrain;
    protected AgentParams AgentParams => _AgentParams;

    public Agent(ref Graph graph, TerrainMesh terrain, AgentParams agentParams)
    {
        _Graph = graph;
        _Terrain = terrain;
        _AgentParams = agentParams;

        _CurrentTokens = agentParams.Tokens;
    }

    public abstract void Initialize();

    /// <summary>
    /// update the agent
    /// </summary>
    /// <returns>if completed</returns>
    public abstract bool Update();
}
