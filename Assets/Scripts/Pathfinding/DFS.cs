﻿using System.Collections.Generic;

public static class DFS
{
    public static List<Vertex> PathTo(Graph graph, Vertex start, Vertex goal, int maxSteps = int.MaxValue)
    {
        Stack<Vertex> open = new Stack<Vertex>();

        graph.InitializeVertices();

        Vertex current = start;
        current.IsVisited = true;
        open.Push(current);

        int steps = 0;

        while (open.Count > 0)
        {
            current = open.Pop();

            if (current.Equals(goal) || steps >= maxSteps)
                return FindPath(start, current);

            foreach (Edge edge in current.Edges)
            {
                Vertex neighbour = edge.To;

                if (!neighbour.IsVisited)
                {
                    neighbour.IsVisited = true;
                    neighbour.Parent = current;

                    if (!open.Contains(neighbour))
                        open.Push(neighbour);
                }
            }

            ++steps;
        }

        return new List<Vertex>(); // Return empty path if none is found
    }

    private static List<Vertex> FindPath(Vertex start, Vertex end) // Reconstruct path
    {
        List<Vertex> path = new List<Vertex>();
        Vertex current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Add(start);
        path.Reverse();

        return path;
    }
}
