using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public List<Vertex> Neighbours { get; private set; }
    public List<Edge> Edges { get; private set; }

    public Vertex Parent { get; set; }
    public Vector3 Position { get; private set; }

    public bool IsVisited { get; set; }

    public float F => G + H;     // Total Cost
    public float G { get; set; } // Distance from current to start
    public float H { get; set; } // Distance from current to end

    public int EdgeCount => Edges.Count;

    public Vertex(float x, float y, float z)
    {
        Position = new Vector3(x, y, z);

        Neighbours = new List<Vertex>();
        Edges = new List<Edge>();

        IsVisited = false;

        G = float.PositiveInfinity;
        H = float.PositiveInfinity;
    }
    public Vertex(Vector3 position) : this(position.x, position.y, position.z) { }

    public void SetPosition(Vector3 position)
    {
        Position = position;
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
