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
        _Position = Terrain.RandomPosition(Terrain.Mountains, Terrain.Width, Terrain.Height);
        _Visited = new List<Vertex>();
    }

    public override bool Update(float deltaTime)
    {
        if (_CurrentTokens-- <= 0)
            return true;

        Vertex current = Graph.AtPos(_Position);
        _Visited.Add(current);

        // squash the area near current position
        // the farther away from current position, the less squashed the vertex is

        List<(Vertex, float)> squashedArea = new List<(Vertex, float)>();
        for (int x = -Params.AreaOfEffect; x <= Params.AreaOfEffect; x++)
        {
            for (int z = -Params.AreaOfEffect; z <= Params.AreaOfEffect; z++)
            {
                Vector2Int pos = _Position + new Vector2Int(x, z);

                if (!Graph.WithinBoard(pos))
                    continue;

                Vertex neighbour = Graph.AtPos(pos);
                float distance = Graph.DiagonalDistance2D(current.LocalPosition, neighbour.LocalPosition);

                if (distance > Params.AreaOfEffect)
                    continue;

                // find the perpendicular vector
                Vector3 direction = (neighbour.WorldPosition - current.WorldPosition).normalized;
                Vector3 perpendicular = (direction != Vector3.zero) ? Quaternion.LookRotation(direction) * Vector3.up : Vector3.up;

                float effect = distance / Params.AreaOfEffect;
                float angle = 1.0f - (Vector3.Angle(perpendicular, Vector3.up) / 90.0f);

                squashedArea.Add((neighbour, -Params.DiminishingCurve.Evaluate(effect) * angle * Params.Strength));
            }
        }

        foreach ((Vertex, float) squashed in squashedArea)
            Graph.AtPos(squashed.Item1.LocalPosition).WorldPosition.y += squashed.Item2;

        // find the next vertex to go to

        PriorityQueue<Vertex> viableVertices = new MaxHeap<Vertex>();
        foreach (Vertex neighbour in current.Neighbours)
        {
            if (_Visited.Contains(neighbour)) // can't go back to already visited vertices
                continue;

            Vector3 direction = (neighbour.WorldPosition - current.WorldPosition).normalized;

            if (Vector3.Dot(direction, Vector3.down) < Params.Leniency) // filter neighbouring vertices by their angle
                continue;

            viableVertices.Enqueue(neighbour, Vector3.Angle(direction, Vector3.up)); // the higher the angle, the lower the vertex is
        }

        if (viableVertices.Count == 0) // if no more viable vertices, then we are done
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

        float posProc = Mathf.InverseLerp(Terrain.Min, Terrain.Max, newVertex.WorldPosition.y);

        return Terrain.BeachesRange.IsInRange(posProc); // run until we have reached the beach
    }
}
