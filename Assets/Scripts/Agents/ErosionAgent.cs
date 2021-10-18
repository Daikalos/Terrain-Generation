using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[System.Serializable]
public class ErosionAgent : Agent
{
    private Vector2Int _Position;
    private List<Vertex> _Visited;

    private ErosionParams Params => (ErosionParams)AgentParams;

    public ErosionAgent(ref Graph graph, TerrainMesh terrain, AgentParams agentParams)
            : base(ref graph, terrain, agentParams) { }

    public override void Initialize()
    {
        _Position = Terrain.Mountains[StaticRandom.Range(0, Terrain.Mountains.Count)];
        _Visited = new List<Vertex>();
    }

    public override bool Update(float deltaTime)
    {
        Vertex current = Graph.AtPos(_Position);
        _Visited.Add(current);

        // squash the area near current position
        // the farther away from current position, the less squashed the vertex is
        for (int x = -Params.AreaOfEffect; x <= Params.AreaOfEffect; ++x)
        {
            for (int z = -Params.AreaOfEffect; z <= Params.AreaOfEffect; ++z)
            {
                Vector2Int pos = _Position + new Vector2Int(x, z);

                if (!Graph.WithinBoard(pos))
                    continue;

                Vertex neighbour = Graph.AtPos(pos);

                float strength = StaticRandom.Range(Params.InverseStrength / 4.0f, Params.InverseStrength);

                float heightOffset = neighbour.WorldPosition.y - current.WorldPosition.y;
                neighbour.WorldPosition.y += -1.0f / ((new Vector3(x, heightOffset, z).magnitude + 1) * strength);
            }
        }

        // find the next vertex to go to

        PriorityQueue<Vertex> viableVertices = new MaxHeap<Vertex>();
        foreach (Vertex neighbour in current.Neighbours)
        {
            if (_Visited.Contains(neighbour)) // can't go back to already visited vertices
                continue;

            Vector3 distanceVector = neighbour.WorldPosition - current.WorldPosition;

            if (Vector3.Dot(distanceVector.normalized, Vector3.down) < Params.Leniency) // filter neighbouring vertices by their angle
                continue;

            viableVertices.Enqueue(neighbour, Vector3.Angle(distanceVector, Vector3.up)); // the higher the angle, the lower the vertex is
        }

        if (viableVertices.Count == 0)
            return true;

        Vertex newVertex = null; // new vertex to go to

        int i = 0;
        while (newVertex == null)
        {
            float chance = viableVertices.Priority(i) / 180.0f;
            if (StaticRandom.Range(0.0f, 1.0f) < chance) // the lower the vertex, the higher the chance to get selected
            {
                newVertex = viableVertices[i];
            }

            i = (i + 1) % viableVertices.Count;
        }

        _Position = newVertex.LocalPosition;

        float posProc = Mathf.InverseLerp(Terrain.Min, Terrain.Max, Graph.AtPos(_Position).WorldPosition.y);

        return Terrain.BeachesRange.IsInRange(posProc) || ++_UsedTokens >= Params.Tokens; // run until we have reached the beach or ran out of tokens to use
    }
}
