using UnityEngine;
using System.Collections.Generic;

public class BlockInformationContainer {

    #region Enum
    public enum WhoHaveTheBlock
    {
        NONE, NORTH, SOUTH
    };
    public static WhoHaveTheBlock BoolToWhoHaveTheBlock(bool goingNorth)
    {
        return goingNorth ? WhoHaveTheBlock.NORTH : WhoHaveTheBlock.SOUTH;
    }
    public static bool WhoHaveTheBlockToBool(WhoHaveTheBlock item)
    {
        switch (item)
        {
            case WhoHaveTheBlock.NORTH:
                return true;
            case WhoHaveTheBlock.SOUTH:
                return false;
            default:
                throw new System.Exception("Cant connvert WhoHaveTheBlock.NONE to bool");
        }
    }
    public static WhoHaveTheBlock NegativeOfBlockDir(WhoHaveTheBlock item)
    {
        switch (item)
        {
            case WhoHaveTheBlock.NORTH:
                return WhoHaveTheBlock.SOUTH;
            case WhoHaveTheBlock.SOUTH:
                return WhoHaveTheBlock.NORTH;
            default:
                return WhoHaveTheBlock.NONE;
        }
    }
    private List<SoldierContain> GetSoldierListByWhoOnBlockEnum(WhoHaveTheBlock whoHaveTheBlock)
    {
        return WhoHaveTheBlockToBool(whoHaveTheBlock) ? goingNorthBlocks : goingSouthBlocks;
    }
    #endregion

    private static readonly int METER_PER_SOLDIER = 2;
    public static readonly Vector3 DEAD_VECTOR = new Vector3(float.MaxValue, float.MinValue, float.MaxValue);

    public WhoHaveTheBlock BlockConqureBy_GoingDirection
    {
        get;
        set;
    }
    public int MaxPeopleInBlock
    {
        get;
        set;
    }
    public int CurrentPlayerInBlock
    {
        get;
        set;
    }
    public Vector3 DirOfStandingInBlock
    {
        get;
        set;
    }
    public Vector3 StartOfBlockingGoingNorth
    {
        get;
        set;
    }
    public Vector3 StartOfBlockingGoingSouth
    {
        get;
        set;
    }
    public int NumberOfBlock
    {
        get;
        set;
    }
    public Situation Situation
    {
        get
        {
            return new Situation(CurrentPlayerInBlock, BlockConqureBy_GoingDirection);
        }
    }

    private List<SoldierContain> goingNorthBlocks;
    private List<SoldierContain> goingSouthBlocks;

    

    public BlockInformationContainer(int sizeOfBlock, Vector3 dirOfBlock , Vector3 startOfBlockGoingNorth , Vector3 startOfBlockGoingSouth)
    {
        MaxPeopleInBlock = sizeOfBlock / METER_PER_SOLDIER;
        CurrentPlayerInBlock = 0;
        BlockConqureBy_GoingDirection = WhoHaveTheBlock.NONE;
        DirOfStandingInBlock = dirOfBlock;
        StartOfBlockingGoingNorth = startOfBlockGoingNorth;
        StartOfBlockingGoingSouth = startOfBlockGoingSouth;
        goingNorthBlocks = new List<SoldierContain>();
        goingSouthBlocks = new List<SoldierContain>();
        CreateBlocksForSoldiers();
    }

    public void Clear(bool nodeInCorner = false)
    {
        CurrentPlayerInBlock = 0;
        if(!nodeInCorner)
            BlockConqureBy_GoingDirection = WhoHaveTheBlock.NONE;
        for (int i = 0; i < MaxPeopleInBlock; i++)
        {
            goingNorthBlocks[i].Clear();
            goingSouthBlocks[i].Clear();
        }
    }


    #region Create blocks for soldier to stand
    private void CreateBlocksForSoldiers()
    {
        for (int i = 0; i < MaxPeopleInBlock; i++)
        {
            goingNorthBlocks.Add(new SoldierContain(PosInBlockCalc(i, true )));
            goingSouthBlocks.Add(new SoldierContain(PosInBlockCalc(i, false)));
        }
    }
    private Vector3 PosInBlockCalc(int num)
    {
        return PosInBlockCalc(num, WhoHaveTheBlockToBool(BlockConqureBy_GoingDirection));
    }
    private Vector3 PosInBlockCalc(int num, bool goingNorth)
    {
        return (goingNorth ? StartOfBlockingGoingNorth : StartOfBlockingGoingSouth) + ((goingNorth ? -1 : 1) * DirOfStandingInBlock * METER_PER_SOLDIER * num);
    }
    #endregion

    #region Soldier managment in block
    public bool CanContainAnotherPlayer( WhoHaveTheBlock whereSoldierGo_ByDirection)
    {
        if (whereSoldierGo_ByDirection == WhoHaveTheBlock.NONE)
            return true;
        if (whereSoldierGo_ByDirection == NegativeOfBlockDir(BlockConqureBy_GoingDirection))
            return false;
        return !(MaxPeopleInBlock == CurrentPlayerInBlock);
    }
    public bool RemoveSoldier(Soldier soldierToRemove , WhoHaveTheBlock whereSoldierStand)
    {
        if (BlockConqureBy_GoingDirection == whereSoldierStand)
        {
            List<SoldierContain> workWith = GetSoldierListByWhoOnBlockEnum(whereSoldierStand);

            foreach (SoldierContain container in workWith)
            {
                if (container.SoldierOnBlock == soldierToRemove)
                {
                    container.SoldierOnBlock = null;
                    CurrentPlayerInBlock--;
                    if (CurrentPlayerInBlock == 0)
                    {
                        BlockConqureBy_GoingDirection = WhoHaveTheBlock.NONE;
                    }
                    //soldierToRemove.NumberOfBlockIn = -1;
                    return true;
                }
            }

        }
        return false;
    }
    public Vector3 AddSoldier(Soldier soldierToEnterToBlock , WhoHaveTheBlock whereSoldierStand)
    {

        if (CanContainAnotherPlayer(whereSoldierStand))
        {
            if (BlockConqureBy_GoingDirection == WhoHaveTheBlock.NONE)
                BlockConqureBy_GoingDirection = whereSoldierStand;

            List<SoldierContain> workWith = GetSoldierListByWhoOnBlockEnum(whereSoldierStand);

            foreach (SoldierContain container in workWith)
            {
                if (container.SoldierOnBlock == null)
                {
                    CurrentPlayerInBlock++;
                    container.SoldierOnBlock = soldierToEnterToBlock;
                    soldierToEnterToBlock.NumberOfBlockIn = NumberOfBlock;
                    return container.PlaceOnBlock;
                }
            }
        }

        return DEAD_VECTOR;
    }
    #endregion

    private class SoldierContain
    {
        public Vector3 PlaceOnBlock
        {
            get;
            set;
        }
        public Soldier SoldierOnBlock
        {
            get;
            set;
        }

        public SoldierContain(Vector3 placeInWorld)
        {
            PlaceOnBlock = placeInWorld;
        }

        public void Clear()
        {
            SoldierOnBlock = null;
        }
    }


    public List<Vector3> GetBlocksToStandForSoldiers_ForDebug(bool goingNorth)
    {
        List<SoldierContain> toWorkWith = goingNorth ? goingNorthBlocks : goingSouthBlocks;
        List<Vector3> toReturn = new List<Vector3>();

        foreach (SoldierContain soldierContain in toWorkWith)
            toReturn.Add(soldierContain.PlaceOnBlock);

        return toReturn;
    }
}
