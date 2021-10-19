using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[System.Serializable]
public class PlainAgent : Agent
{
    private Vector2Int _StartPosition;
    private Vector2Int _Position;
    private Vector2Int _Direction;

    private bool _FoundGoodPosition = true;

    public PlainParams Params => (PlainParams)AgentParams;

    public PlainAgent(ref Graph graph, TerrainMesh terrain, AgentParams agentParams)
        : base(ref graph, terrain, agentParams) { }

    public override void Initialize()
    {
        int maxSearches = 256;
        do
        {
            _Position = FindTop(Terrain.RandomPosition(Terrain.Plains, Terrain.Width, Terrain.Height));
        }
        while (!Terrain.PlainsRange.IsInRange(Mathf.InverseLerp(Terrain.Min, Terrain.Max, Graph.AtPos(_Position).WorldPosition.y)) && --maxSearches > 0);

        if (maxSearches <= 0)
            _FoundGoodPosition = false;

        _StartPosition = _Position;
        _Direction = new Vector2Int(StaticRandom.Range(-1, 2), StaticRandom.Range(-1, 2));
    }

    public override bool Update()
    {
        if (_CurrentTokens-- <= 0 || !_FoundGoodPosition)
            return true;

        Vertex current = Graph.AtPos(_Position);

        float avg = 0.0f;
        int count = 0;
        for (int x = -Params.AreaOfEffect; x <= Params.AreaOfEffect; ++x)
        {
            for (int z = -Params.AreaOfEffect; z <= Params.AreaOfEffect; ++z)
            {
                Vector2Int pos = _Position + new Vector2Int(x, z);

                if (!Graph.WithinBoard(pos))
                    continue;

                Vertex neighbour = Graph.AtPos(pos);
                float distance = Graph.DiagonalDistance2D(current.LocalPosition, neighbour.LocalPosition);

                if (distance > Params.AreaOfEffect)
                    continue;

                avg += neighbour.WorldPosition.y;
                ++count;
            }
        }
        avg /= count;

        for (int x = -Params.AreaOfEffect; x <= Params.AreaOfEffect; ++x)
        {
            for (int z = -Params.AreaOfEffect; z <= Params.AreaOfEffect; ++z)
            {
                Vector2Int pos = _Position + new Vector2Int(x, z);

                if (!Graph.WithinBoard(pos))
                    continue;

                Vertex neighbour = Graph.AtPos(pos);
                float distance = Graph.DiagonalDistance2D(current.LocalPosition, neighbour.LocalPosition);

                if (distance > Params.AreaOfEffect)
                    continue;

                float effect = distance / Params.AreaOfEffect;

                neighbour.WorldPosition.y = Mathf.SmoothStep(neighbour.WorldPosition.y, avg, 
                    Params.Diminish.Evaluate(effect) * Params.Smoothing);
            }
        }

        if (StaticRandom.Range(0.0f, 1.0f) <= Params.SwitchDirectionChance)
        {
            float angle = StaticRandom.Range(0.0f, 2.0f * Mathf.PI);
            Vector2 pointInCircle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 randomPoint = _StartPosition + pointInCircle * Params.AreaOfEffect;
            _Direction = Vector2Int.RoundToInt((randomPoint - _Position).normalized);
        }

        _Position += _Direction * (int)Params.MoveMagnitude;

        return !Graph.WithinBoard(_Position);
    }

    private Vector2Int FindTop(Vector2Int start)
    {
        Vector2Int result = start;
        int maxSearches = 256;

        while (--maxSearches > 0)
        {
            Vertex current = Graph.AtPos(result);

            PriorityQueue<Vertex> viableVertices = new MaxHeap<Vertex>();
            foreach (Vertex neighbour in current.Neighbours)
            {
                Vector3 direction = (neighbour.WorldPosition - current.WorldPosition).normalized;

                if (Vector3.Dot(direction, Vector3.down) > 0.0f)
                    continue;

                viableVertices.Enqueue(neighbour, Vector3.Angle(direction, Vector3.up));
            }

            if (viableVertices.Count == 0)
                break;

            result = viableVertices.Dequeue().LocalPosition;
        }

        return result;
    }
}
