using UnityEngine;
using System.Collections;

public class BlockPlayerContainer
{
    public int BlockNumber
    {
        get;
        set;
    }
    public int HowMuchPeople
    {
        get;
        set;
    }


    public BlockPlayerContainer(int blockNumber, int peopleNum)
    {
        HowMuchPeople = peopleNum;
        BlockNumber = blockNumber;
    }

    public override bool Equals(object obj)
    {
        BlockPlayerContainer other = (BlockPlayerContainer)obj;
        if (HowMuchPeople == other.HowMuchPeople && BlockNumber == other.HowMuchPeople)
            return true;
        return false;
    }

    public void AddAnotherPlayer()
    {
        HowMuchPeople = HowMuchPeople + 1;
    }

}