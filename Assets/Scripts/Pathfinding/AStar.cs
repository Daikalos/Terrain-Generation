using System.Collections.Generic;

public static class AStar
{
    public static List<Vertex> PathTo(Graph graph, PriorityQueue<Vertex> priority, Vertex start, Vertex goal, int maxSteps = int.MaxValue)
    {
        PriorityQueue<Vertex> open = priority;
            
        graph.InitializeVertices();

        Vertex current = start;
        open.Enqueue(current, current.F);
        current.G = 0;

        int steps = 0;

        while (open.Count > 0)
        {
            current = open.Dequeue();

            if (!current.IsVisited)
            {
                current.IsVisited = true;

                if (current.Equals(goal) || steps >= maxSteps)
                    return FindPath(start, current);

                foreach (Edge edge in current.Edges)
                {
                    Vertex neighbour = edge.To;

                    float gScore = current.G + Graph.DiagonalDistance(current, neighbour);
                    if (gScore < neighbour.G)
                    {
                        neighbour.G = gScore;
                        neighbour.H = Graph.DiagonalDistance(neighbour, goal);

                        neighbour.Parent = current;

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, neighbour.F);
                    }
                }

                ++steps;
            }
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
