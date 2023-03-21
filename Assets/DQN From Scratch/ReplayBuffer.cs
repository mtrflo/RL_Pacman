using System.Collections.Generic;

public class ReplayBuffer<T>
{
    private List<T> buffer;
    private int maxSize;

    public ReplayBuffer(int maxSize)
    {
        buffer = new List<T>();
        this.maxSize = maxSize;
    }

    public void Add(T experience)
    {
        if (buffer.Count >= maxSize)
        {
            buffer.RemoveAt(0);
        }
        buffer.Add(experience);
    }

    public T GetRandomExperience()
    {
        int index = UnityEngine.Random.Range(0, buffer.Count);
        return buffer[index];
    }

    public int Size()
    {
        return buffer.Count;
    }

    public void Clear()
    {
        buffer.Clear();
    }
}