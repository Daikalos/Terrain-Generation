using System.Collections.Generic;

abstract class PriorityQueue<T> // Min Heap
{
    protected class Element
    {
        public T Item { get; private set; }
        public float Priority { get; private set; }

        public Element(T item, float priority)
        {
            Item = item;
            Priority = priority;
        }
    }

    protected int Parent(int pos) => (pos - 1) / 2;
    protected int Left(int pos) => (2 * pos) + 1;
    protected int Right(int pos) => (2 * pos) + 2;

    public int Count => priorityQueue.Count;

    protected List<Element> priorityQueue;

    public T this[int i]
    {
        get { return priorityQueue[i].Item; }
    }

    public PriorityQueue()
    {
        priorityQueue = new List<Element>();
    }

    public abstract void Enqueue(T Element, float priority);
    public abstract T Dequeue();

    public bool Contains(T Element)
    {
        for (int i = 0; i < priorityQueue.Count; i++)
        {
            if (priorityQueue[i].Item.Equals(Element))
                return true;
        }
        return false;
    }

    protected void Swap(int first, int second)
    {
        Element tmp = priorityQueue[first];
        priorityQueue[first] = priorityQueue[second];
        priorityQueue[second] = tmp;
    }
}
