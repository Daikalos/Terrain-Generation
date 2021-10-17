using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlainAgent : Agent
{
    public PlainAgent(ref Graph graph, TerrainMesh terrain, AgentParams agentParams) : base(ref graph, terrain, agentParams)
    { }

    public override void Initialize()
    {

    }

    public override bool Update()
    {


        return true;
    }
}
