using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private SortedDictionary<float, Queue<T>> elements = new SortedDictionary<float, Queue<T>>();

    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!elements.ContainsKey(priority))
            elements[priority] = new Queue<T>();

        elements[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        var firstPair = elements.First();
        var item = firstPair.Value.Dequeue();

        if (firstPair.Value.Count == 0)
            elements.Remove(firstPair.Key);

        Count--;
        return item;
    }

    public float Priority(T item)
    {
        foreach (var pair in elements)
        {
            if (pair.Value.Contains(item))
                return pair.Key;
        }
        return float.MaxValue;
    }

    public T Peek()
    {
        return elements.First().Value.Peek();
    }

    public bool Contains(T item)
    {
        foreach (var pair in elements)
        {
            if (pair.Value.Contains(item)) return true;
        }
        return false;
    }

   public void Remove(T item)
{
    List<float> keysToRemove = new List<float>();
    foreach (var pair in elements)
    {
        if (pair.Value.Contains(item))
        {
            // Rebuild the queue without the removed item
            var updatedQueue = new Queue<T>(pair.Value.Where(x => !EqualityComparer<T>.Default.Equals(x, item)));

            if (updatedQueue.Count > 0)
                elements[pair.Key] = updatedQueue; // Replace the queue
            else
                keysToRemove.Add(pair.Key); // Mark for removal
        }
    }

    // Remove empty keys
    foreach (var key in keysToRemove)
    {
        elements.Remove(key);
    }

    Count--;
}
}
