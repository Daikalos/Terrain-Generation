using System.Linq;

class MaxHeap<T> : PriorityQueue<T>
{
    public override void Enqueue(T Element, float priority)
    {
        priorityQueue.Add(new Element(Element, priority)); // Add to end of list
        MoveUp(Count - 1);
    }

    public override T Dequeue()
    {
        if (Count <= 0) // No data available
            return default;

        Element root = priorityQueue.First();

        priorityQueue[0] = priorityQueue[Count - 1]; // Replace root with element at end
        priorityQueue.RemoveAt(Count - 1);           // Remove element at end

        MoveDown(0); // After previous root is removed, move the current one down

        return root.Item;
    }

    private void MoveUp(int pos)
    {
        if (pos <= 0) // We have reached root
            return;

        int parent = Parent(pos);
        if (priorityQueue[pos].Priority > priorityQueue[parent].Priority)
        {
            Swap(pos, parent);
            MoveUp(parent);
        }
    }

    private void MoveDown(int pos)
    {
        int left = Left(pos);
        int right = Right(pos);

        int smallest = pos;
        if (left < Count && priorityQueue[left].Priority > priorityQueue[smallest].Priority)
        {
            smallest = left;
        }
        if (right < Count && priorityQueue[right].Priority > priorityQueue[smallest].Priority)
        {
            smallest = right;
        }

        if (smallest != pos)
        {
            Swap(pos, smallest);
            MoveDown(smallest);
        }
    }
}
