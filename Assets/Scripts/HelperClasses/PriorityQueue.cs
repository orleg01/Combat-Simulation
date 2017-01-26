using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class PriorityQueue<T>
{
    protected IComparer<T> comparer;
    protected T[] heap;

    public int Count { get; protected set; }
    public PriorityQueue() : this(null) { }
    public PriorityQueue(int capacity) : this(capacity, null) { }
    public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }
    public PriorityQueue(int capacity, IComparer<T> comparer)
    {
        this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
        this.heap = new T[capacity];
    }
    public abstract void push(T v);

    public T pop()
    {
        var v = top();
        heap[0] = heap[--Count];
        if (Count > 0)
            SiftDown(0);
        return v;
    }
    public T top()
    {
        if (Count > 0) return 
                heap[0];
        throw new InvalidOperationException("The heap is empty");
    }
    protected void SiftUp(int n)
    {
        var v = heap[n];
        for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2)
            heap[n] = heap[n2];
        heap[n] = v;
    }
    protected void SiftDown(int n)
    {
        var v = heap[n];
        for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
        {
            if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0)
                n2++;
            if (comparer.Compare(v, heap[n2]) >= 0)
                break;
            heap[n] = heap[n2];
        }
        heap[n] = v;
    }

    public abstract void ChangePlace(T item);
    public abstract bool Contain(T item);

    public T getNearTheHeigher(int num)
    {
        return heap[num];
    }

    public T[] GetArr()
    {
        T[] toReturn = new T[Count];
        Array.Copy(heap, toReturn , Count);
        return toReturn;

    }

}


