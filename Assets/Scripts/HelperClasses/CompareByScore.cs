using UnityEngine;
using System.Collections.Generic;
using System;

public class CompareByScore : IComparer<MoveToScoreManager>
{
    public int Compare(MoveToScoreManager me, MoveToScoreManager other)
    {
        if (me == other)
            return 0;
        if (me.Score > other.Score)
            return 1;
        else if (me.Score < other.Score)
            return -1;
        else
        {
            float myUnrelevantData = me.GetComperatorHelper;
            float otherUnrelevantData = other.GetComperatorHelper;

            if (myUnrelevantData < otherUnrelevantData)
                return 1;
            else
                return -1;
        }

    }

}
