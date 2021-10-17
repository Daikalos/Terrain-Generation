using System;
using UnityEngine;

/// <summary>
/// weighted graph
/// </summary>
public class Graph
{
    private Vertex[] _Vertices;

    public Vertex[] Vertices => _Vertices;

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
        for (int x = 0; x < Width; ++x) // Add all vertices
        {
            for (int z = 0; z < Height; ++z)
            {
                _Vertices[x + z * Width] = new Vertex(new Vector3(x, 0.0f, z), new Vector2Int(x, z));
            }
        }
    }
    private void AddEdges()
    {
        for (int x = 0; x < Width; ++x) // Add all edges
        {
            for (int z = 0; z < Height; ++z)
            {
                for (int i = -1; i <= 1; ++i)
                {
                    for (int j = -1; j <= 1; ++j)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        if (WithinBoard(x + i, z + j))
                        {
                            Vertex vertex = AtPos(x, z);
                            Vertex neighbour = AtPos(x + i, z + j);

                            vertex.AddNeighbour(neighbour);

                            new Edge(vertex, neighbour, DiagonalDistance(vertex, neighbour));
                        }
                    }
                }
            }
        }
    }

    public void SetVertices(Vector3[] vertices)
    {
        _Vertices = new Vertex[Width * Height];
        for (int x = 0; x < Width; ++x) // Add all vertices
        {
            for (int z = 0; z < Height; ++z)
            {
                int i = x + z * Width;
                _Vertices[i] = new Vertex(vertices[i], new Vector2Int(x, z));
            }
        }
        AddEdges();
    }
    public void SetVertices(float[] heights)
    {
        _Vertices = new Vertex[Width * Height];
        for (int x = 0; x < Width; ++x) // Add all vertices
        {
            for (int z = 0; z < Height; ++z)
            {
                int i = x + z * Width;
                _Vertices[i] = new Vertex(new Vector3(x, heights[i], z), new Vector2Int(x, z));
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

        vertex.WorldPosition.y = position.y;
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
        if (!WithinBoard(x, y))
            return null;

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
        return WithinBoard(vertex.LocalPosition);
    }

    public static float ManhattanDistance(Vertex from, Vertex to)
    {
        return Math.Abs(to.WorldPosition.x - from.WorldPosition.x) +
               Math.Abs(to.WorldPosition.y - from.WorldPosition.y) +
               Math.Abs(to.WorldPosition.z - from.WorldPosition.z);
    }
    public static float DiagonalDistance(Vertex from, Vertex to)
    {
        return (float)Math.Sqrt(Math.Pow(to.WorldPosition.x - from.WorldPosition.x, 2) +
                                Math.Pow(to.WorldPosition.y - from.WorldPosition.y, 2) +
                                Math.Pow(to.WorldPosition.z - from.WorldPosition.z, 2));
    }
}
