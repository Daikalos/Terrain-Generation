using System.Collections.Generic;

public class Dijkstra
{
    public static List<Vertex> PathTo(Graph graph, Vertex start, Vertex goal)
    {
        PriorityQueue<Vertex> open = new MinHeap<Vertex>();

        graph.InitializeVertices();

        Vertex current = start;
        open.Enqueue(current, current.F);

        current.G = 0;

        while (open.Count > 0)
        {
            current = open.Dequeue();

            if (!current.IsVisited)
            {
                current.IsVisited = true;
                    
                if (current.Equals(goal))
                    return FindPath(start, goal);

                foreach (Edge edge in current.Edges)
                {
                    Vertex neighbour = edge.To;

                    float gScore = current.G + edge.Weight;
                    if (gScore < neighbour.G)
                    {
                        neighbour.G = gScore;
                        neighbour.H = 0.0f;

                        neighbour.Parent = current;

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, neighbour.F);
                    }
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
