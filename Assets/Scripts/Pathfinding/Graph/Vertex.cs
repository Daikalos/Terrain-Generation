using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public List<Vertex> Neighbours { get; private set; }
    public List<Edge> Edges { get; private set; }

    public Vertex Parent;
    public Vector3 WorldPosition;
    public Vector2Int LocalPosition;

    public bool IsVisited { get; set; }

    public float F => G + H;     // Total Cost
    public float G { get; set; } // Distance from current to start
    public float H { get; set; } // Distance from current to end

    public int EdgeCount => Edges.Count;

    public Vertex(Vector3 worldPos, Vector2Int localPos)
    {
        WorldPosition = worldPos;
        LocalPosition = localPos;

        Neighbours = new List<Vertex>();
        Edges = new List<Edge>();

        IsVisited = false;

        G = float.PositiveInfinity;
        H = float.PositiveInfinity;
    }

    public void ClearNeighbours()
    {
        Neighbours.Clear();
    }
    public void AddNeighbour(Vertex vertex)
    {
        Neighbours.Add(vertex);
    }
    public void RemoveNeighbour(Vertex vertex)
    {
        Neighbours.Remove(vertex);
    }

    public void ClearEdges()
    {
        Edges.Clear();
    }
    public void AddEdge(Edge edge)
    {
        if (edge == null)
            return;

        Edges.Add(edge);
    }
    public void RemoveEdge(Edge edge)
    {
        if (edge == null)
            return;

        Edges.Remove(edge);
    }
}
