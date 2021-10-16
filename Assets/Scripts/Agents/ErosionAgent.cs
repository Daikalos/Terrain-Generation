using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ErosionAgent : Agent
{
    [Space(5)]
    [SerializeField] private float _Leniency = 0.30f;
    [SerializeField] private float _InverseStrength = 3.0f;
    [SerializeField] private int _Area = 4;

    private Vector2Int _Position;
    private List<Vertex> _Visited;

    private int steps = 0;

    public override void Initialize(ref Graph graph)
    {
        base.Initialize(ref graph);

        _Position = new Vector2Int(StaticRandom.Range(0, graph.Width), StaticRandom.Range(0, graph.Height));

        _Visited = new List<Vertex>();
    }

    public override bool Update()
    {
        Vertex current = _Graph.AtPos(_Position);

        _Visited.Add(current);

        Vertex greatest = null;
        float distance = -float.MaxValue;
   
        foreach (Vertex neighbour in current.Neighbours)
        {
            if (_Visited.Contains(neighbour))
                continue;

            Vector3 distanceVector = neighbour.WorldPosition - current.WorldPosition;

            if (Vector3.Dot(distanceVector, Vector3.down) < _Leniency)
                continue;

            float distanceTo = Graph.DiagonalDistance(current, neighbour);
            if (distanceTo > distance)
            {
                greatest = neighbour;
                distance = distanceTo;
            }
        }

        if (greatest == null)
            return true;

        for (int x = -_Area; x <= _Area; ++x)
        {
            for (int z = -_Area; z <= _Area; ++z)
            {
                Vector2Int pos = _Position + new Vector2Int(x, z);

                if (!_Graph.WithinBoard(pos))
                    continue;

                Vertex vertex = _Graph.AtPos(pos);

                vertex.WorldPosition.y += -1.0f / ((Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)) + 1) * _InverseStrength);
            }
        }

        _Position = greatest.LocalPosition;

        return ++steps >= _Lifetime;
    }
}
