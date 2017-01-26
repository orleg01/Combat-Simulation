using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PriorityQueueOfMoveToScore : PriorityQueue<MoveToScoreManager> {
    public PriorityQueueOfMoveToScore(IComparer<MoveToScoreManager> comparer) : base(comparer)
    {
    }

    public override void ChangePlace(MoveToScoreManager item)
    {
        for (int i = 0; i < heap.Length; i++)
            if (item.movesTheSame(heap[i]))
            {
                SiftUp(i);
                SiftDown(i);
                return;
            }
    }

    public override bool Contain(MoveToScoreManager item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (item.movesTheSame(heap[i]))
                return true;
        }
        return false;
    }

    public override void push(MoveToScoreManager v)
    {
        if(Contain(v))
            return;
        if (Count >= heap.Length)
            Array.Resize(ref heap, Count * 2);
        heap[Count] = v;
        SiftUp(Count++);
    }
}
