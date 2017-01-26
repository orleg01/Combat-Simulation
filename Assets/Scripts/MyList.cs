using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MyList
{
    private Node NorthStart
    {
        get;
        set;
    }
    private Node SouthStart
    {
        get;
        set;
    }
    public int NumberItemInList
    {
        get;
        set;
    }
    public bool GameIsFinish
    {
        get
        {
            Node checkNorth = NorthStart;
            Node checkSouth = SouthStart;
            if (checkNorth.SouthNode.WhosHaveTheBlock == BlockInformationContainer.WhoHaveTheBlock.NORTH)
                return true;
            else if (checkSouth.NorthNode.WhosHaveTheBlock == BlockInformationContainer.WhoHaveTheBlock.SOUTH)
                return true;
            return false;

        }
    }

    public MyList(BoxCollider northCol, BlockInformationContainer northInfo, BoxCollider southCol, BlockInformationContainer southInfo)
    {
        NumberItemInList = 2;
        NorthStart = new Node(northInfo, northCol.transform.position.z);
        SouthStart = new Node(southInfo, southCol.transform.position.z);
        SouthStart.NorthNode = NorthStart;
        NorthStart.SouthNode = SouthStart;
    }

    public int addItem(BoxCollider insert , BlockInformationContainer info)
    {
        Node node = new Node(info, insert.gameObject.transform.position.z);
        Node start = SouthStart;

        int check = 0;
        while (start != null )
        {
            check++;
            if (start != node)
                if (start.recognaizeAsNeiberHood(node))
                    break;
            start = start.NorthNode;
        }

        NumberItemInList++;

        return check;
    }

    public Situation[] getSituationOfWorld()
    {
        Node start = SouthStart;
        Situation[] toReturn = new Situation[NumberItemInList];
        int index = 0;

        while (start != null)
        {
            toReturn[index++] = start.Situation;
            start = start.NorthNode;
        }

        return toReturn;
    }

    private Node[] nodeAsArray;
    public void GiveNumberToEachNode()
    {
        Node start = SouthStart;
        int index = 0;
        nodeAsArray = new Node[NumberItemInList];

        while (start != null)
        {
            nodeAsArray[index] = start;
            start.NumberOfBlock = index++;
            start = start.NorthNode;
        }
    }
    public List<int> whereCanGo(BlockInformationContainer.WhoHaveTheBlock whoHaveBlock , int numberOfJumpInBlocks = 1)
    {
        NorthStart.WhosHaveTheBlock = BlockInformationContainer.WhoHaveTheBlock.SOUTH;
        SouthStart.WhosHaveTheBlock = BlockInformationContainer.WhoHaveTheBlock.NORTH;

        List<int> toReturn = new List<int>();
        Node lastNode = null;
        bool goingNorth = BlockInformationContainer.WhoHaveTheBlockToBool(whoHaveBlock);
        int posOrNegative = goingNorth ? 1 : -1;
        Node start = goingNorth ? SouthStart : NorthStart;

        while (start != null)
        {
            if (start.WhosHaveTheBlock == whoHaveBlock)
                lastNode = start;
            start =  goingNorth ? start.NorthNode : start.SouthNode;
        }

        int numberOfNode = lastNode.NumberOfBlock;

        toReturn.Add(numberOfNode);
        for (int i = 1; i < numberOfJumpInBlocks + 1; i++)
        {
            int j = i * posOrNegative;
            if (nodeAsArray[numberOfNode + j].NodeIsFree)
            {
                toReturn.Add(numberOfNode + j);
            }
        }

        int numberOfItr = 0;
        while (numberOfNode > 0 && numberOfNode < NumberItemInList - 1 && numberOfItr < 4)
        {
            numberOfItr++;
            numberOfNode -= posOrNegative;
            toReturn.Add(numberOfNode);
        }

        return toReturn;
    }

    #region New Soldier
    public Vector3 AddSoldier(Soldier sold , int numberOfBlock , BlockInformationContainer.WhoHaveTheBlock whosHaveTheBlock )
    {
        try
        {
            return nodeAsArray[numberOfBlock].AddSoldierToBlock(sold, whosHaveTheBlock);
        }
        catch
        {
            throw new System.Exception("What the fuck");
        }
    }

    public bool RemoveSoldier(Soldier sold, int numberOfBlock , BlockInformationContainer.WhoHaveTheBlock whosHaveTheBlock)
    {
        try
        {
            return nodeAsArray[numberOfBlock].RemoveSoldierFromBlock(sold, whosHaveTheBlock);
        }
        catch
        {
            throw new System.Exception("What the fuck");
        }
    }
    internal Vector3 AddNewSoldier(Soldier soldier, bool goingNorth, BlockInformationContainer.WhoHaveTheBlock whoHaveTheBlock)
    {
        Node whereToEnter = goingNorth ? SouthStart : NorthStart;
        return whereToEnter.AddSoldierToBlock(soldier, whoHaveTheBlock);
    }
    public Vector3 ChangeLocation(Soldier soldier, bool goingNorth , int whereSoldierNeedToGoTo)
    {
        int whereTheSoldierNow = soldier.NumberOfBlockIn;
        BlockInformationContainer.WhoHaveTheBlock whoHaveTheBlock;
        RemoveSoldier(soldier, whereTheSoldierNow, whoHaveTheBlock =  BlockInformationContainer.BoolToWhoHaveTheBlock(goingNorth));
        return AddSoldier(soldier, whereSoldierNeedToGoTo, whoHaveTheBlock);
    }
    #endregion

    public void Clear()
    {
        for (int i = 1; i < nodeAsArray.Length - 1; i++)
        {
            nodeAsArray[i].Clear();
        }
        nodeAsArray[0].Clear(true);
        nodeAsArray[nodeAsArray.Length - 1].Clear(true);
        
    }

    private class Node
    {
        private BlockInformationContainer nodeInfo;
        private Node goNextNorth;
        private Node goNextSouth;



        public float PositionToCompare
        {
            get;
            set;
        }
        public Node NorthNode
        {
            get
            {
                return goNextNorth;
            }
            set
            {
                goNextNorth = value;
            }
        }
        public Node SouthNode
        {
            get
            {
                return goNextSouth;
            }
            set
            {
                goNextSouth = value;
            }
        }
        public Situation Situation
        {
            get
            {
                return nodeInfo.Situation;
            }
        }
        public BlockInformationContainer.WhoHaveTheBlock WhosHaveTheBlock
        {
            get
            {
                return nodeInfo.BlockConqureBy_GoingDirection;
            }
            set
            {
                nodeInfo.BlockConqureBy_GoingDirection = value;
            }
        }
        public int NumberOfBlock
        {
            get
            {
                return nodeInfo.NumberOfBlock;
            }
            set
            {
                nodeInfo.NumberOfBlock = value;
            }
        }
        public bool NodeIsFree
        {
            get
            {
                if (WhosHaveTheBlock == BlockInformationContainer.WhoHaveTheBlock.NONE)
                    return true;
                return false;
            }
        }

        public Node(BlockInformationContainer block, float zPos)
        {
            nodeInfo = block;
            PositionToCompare = zPos;
        }

        public bool recognaizeAsNeiberHood(Node node)
        {

            if (goNextNorth != null && goNextNorth.PositionToCompare > node.PositionToCompare)
            {

                node.goNextNorth = goNextNorth;
                goNextNorth.goNextSouth = node;

                node.goNextSouth = this;
                goNextNorth = node;
                
                return true;
            }

            return false;

        }

        internal Vector3 AddSoldierToBlock(Soldier sold, BlockInformationContainer.WhoHaveTheBlock whosHaveTheBlock)
        {
            return nodeInfo.AddSoldier(sold, whosHaveTheBlock);
        }

        internal bool RemoveSoldierFromBlock(Soldier sold, BlockInformationContainer.WhoHaveTheBlock whosHaveTheBlock)
        {
            return nodeInfo.RemoveSoldier(sold, whosHaveTheBlock);
        }

        public void Clear(bool nodeInCorner = false)
        {
            nodeInfo.Clear(nodeInCorner);
        }

    }
}
