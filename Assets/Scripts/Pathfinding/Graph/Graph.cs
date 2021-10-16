using System;
using UnityEngine;

/// <summary>
/// weighted graph
/// </summary>
public class Graph
{
    private Vertex[] _Vertices;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Graph(int width, int height)
    {
        Width = width;
        Height = height;

        _Vertices = new Vertex[Width * Height];
    }

    public void Generate()
    {
        AddVertices();
        AddEdges();
    }
    private void AddVertices()
    {
        for (int z = 0; z < Height; ++z) // Add all vertices
        {
            for (int x = 0; x < Width; ++x)
            {
                _Vertices[x + z * Width] = new Vertex(x, 0.0f, z);
            }
        }
    }
    private void AddEdges()
    {
        for (int y = 0; y < Height; ++y) // Add all edges
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    if (WithinBoard(x + i, y))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x + i, y);

                        vertex.AddNeighbour(neighbour);

                        new Edge(vertex, neighbour, DiagonalDistance(vertex, neighbour));
                    }
                }
                for (int j = -1; j <= 1; j += 2)
                {
                    if (WithinBoard(x, y + j))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x, y + j);

                        vertex.AddNeighbour(neighbour);

                        new Edge(vertex, neighbour, DiagonalDistance(vertex, neighbour));
                    }
                }
            }
        }
    }

    public void SetVertices(Vector3[] vertices)
    {
        _Vertices = new Vertex[Width * Height];
        for (int y = 0; y < Height; ++y) // Add all vertices
        {
            for (int x = 0; x < Width; ++x)
            {
                int i = x + y * Width;
                _Vertices[i] = new Vertex(vertices[i]);
            }
        }
        AddEdges();
    }
    public void SetVertex(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int z = Mathf.FloorToInt(position.z);

        if (!WithinBoard(x, z))
            return;

        Vertex vertex = _Vertices[x + z * Width];

        vertex.Position.y = position.y;
    }

    public void InitializeVertices()
    {
        foreach (Vertex vertex in _Vertices)
        {
            vertex.IsVisited = false;
            vertex.G = float.PositiveInfinity;
            vertex.H = float.PositiveInfinity;
        }
    }

    public Vertex AtPos(int x, int y)
    {
        return _Vertices[x + y * Width];
    }
    public Vertex AtPos(Vector2Int pos)
    {
        return AtPos(pos.x, pos.y);
    }

    public bool WithinBoard(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= Width || y >= Height);
    }
    public bool WithinBoard(Vector2Int pos)
    {
        return WithinBoard(pos.x, pos.y);
    }
    public bool WithinBoard(Vertex vertex)
    {
        return WithinBoard(Mathf.FloorToInt(vertex.Position.x), Mathf.FloorToInt(vertex.Position.z));
    }

    public static float ManhattanDistance(Vertex from, Vertex to)
    {
        return Math.Abs(to.Position.x - from.Position.x) +
               Math.Abs(to.Position.y - from.Position.y) +
               Math.Abs(to.Position.z - from.Position.z);
    }
    public static float DiagonalDistance(Vertex from, Vertex to)
    {
        return (float)Math.Sqrt(Math.Pow(to.Position.x - from.Position.x, 2) +
                                Math.Pow(to.Position.y - from.Position.y, 2) +
                                Math.Pow(to.Position.z - from.Position.z, 2));
    }
}
