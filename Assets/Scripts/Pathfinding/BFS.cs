using System.Collections.Generic;

public static class BFS
{
    public static List<Vertex> PathTo(Graph graph, Vertex start, Vertex goal)
    {
        Queue<Vertex> open = new Queue<Vertex>();

        graph.InitializeVertices();

        Vertex current = start;
        current.IsVisited = true;

        open.Enqueue(current);

        while (open.Count > 0)
        {
            current = open.Dequeue();

            if (current.Equals(goal))
                return FindPath(start, goal);

            foreach (Edge edge in current.Edges)
            {
                Vertex neighbour = edge.To;

                if (!neighbour.IsVisited)
                {
                    neighbour.IsVisited = true;
                    neighbour.Parent = current;

                    if (!open.Contains(neighbour))
                        open.Enqueue(neighbour);
                }
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
