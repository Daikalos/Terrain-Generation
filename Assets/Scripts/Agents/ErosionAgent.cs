using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ErosionAgent : Agent
{
    private Vector2Int _RandomPosition;
    private Vector2Int _Test;

    private List<Vertex> vertices;
    private int steps = 0;

    public override void Initialize(ref Graph graph)
    {
        base.Initialize(ref graph);

        _RandomPosition = new Vector2Int(StaticRandom.Range(0, graph.Width), StaticRandom.Range(0, graph.Height));
        _Test = new Vector2Int(StaticRandom.Range(0, graph.Width), StaticRandom.Range(0, graph.Height));

        vertices = Dijkstra.PathTo(_Graph, new MaxHeap<Vertex>(), _Graph.AtPos(_RandomPosition), _Graph.AtPos(_Test), 1000);
    }

    public override bool Update()
    {
        vertices[steps++].Position.y--;

        return steps >= vertices.Count;
    }
}
