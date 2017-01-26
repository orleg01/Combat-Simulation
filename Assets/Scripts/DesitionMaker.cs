using UnityEngine;
using System.Collections.Generic;
using System;

public class DesitionMaker
{

    public static Vector3 BAD_VECTOR = new Vector3(-1, -1, -1);

    private Dictionary<long, OptionToGo> dataNorth;
    private Dictionary<long, OptionToGo> dataSouth;

    public DesitionMaker()
    {
        dataNorth = new Dictionary<long, OptionToGo>();
        dataSouth = new Dictionary<long, OptionToGo>();
    }
    internal List<BlockPlayerContainer> WhichWayShouldIChoose(Situation[] situations, bool northPosition , List<int> placesCanGoTo , long key = -1 )
    {
        if(key == -1)
            key = getMyKey(northPosition, situations);

        Dictionary<long, OptionToGo> temp = northPosition ? dataNorth : dataSouth;

        OptionToGo value;
        if (temp.TryGetValue(key, out value))
        {
            return value.getMovesNeeded(placesCanGoTo , northPosition);
        }
        else
        {
            OptionToGo optionToGO;
            temp.Add(key, optionToGO = new OptionToGo());
            return optionToGO.getMovesNeeded(placesCanGoTo , northPosition); 
        }
    }

    #region Generate key functions
    //private static readonly int MAX_BLOCK_IN_MAP = 20;
    private long getMyKey(bool northPos , Situation[] situations)
    {
        long toReturn = 0;
        

        BlockInformationContainer.WhoHaveTheBlock whoAmi = northPos ? BlockInformationContainer.WhoHaveTheBlock.NORTH : BlockInformationContainer.WhoHaveTheBlock.SOUTH;
        BlockInformationContainer.WhoHaveTheBlock whoAgainstMe = !northPos ? BlockInformationContainer.WhoHaveTheBlock.NORTH : BlockInformationContainer.WhoHaveTheBlock.SOUTH; ;
        int sitLen2 = situations.Length * 2;

        for (int i = 0; i < situations.Length; i++)
        {
            if(situations[i].BlockConqureBy_GoingDirection == whoAmi)
                toReturn += (1 << i);
        }

        for (int i = situations.Length; i < sitLen2; i++)
        {
            if (situations[i - situations.Length].BlockConqureBy_GoingDirection == whoAgainstMe)
                toReturn += (1 << i);
        }

        return toReturn;
    }
    #endregion



    public class OptionToGo
    {

        private PriorityQueueOfMoveToScore allMovesAndScores;
        public OptionToGo()
        {
            allMovesAndScores = new PriorityQueueOfMoveToScore(new CompareByScore());
        }

        #region Score manager
        private static Queue<MoveToScoreManager> lastMovesToScore = new Queue<MoveToScoreManager>();
        private static Queue<PriorityQueue<MoveToScoreManager>> priorityQueueForEachMov = new Queue<PriorityQueue<MoveToScoreManager>>();
        private static float lastScoreNorth = 0;
        private static float lastScoreSouth = 0;
        private static void setScore(MoveToScoreManager move , PriorityQueue<MoveToScoreManager> queue , bool north )
        {
            if (lastMovesToScore.Count == 10)
            {
                MoveToScoreManager temp = lastMovesToScore.Dequeue();
                float nextScore = Leader.getCurrentScore(temp.North);
                float newScore = nextScore - (north ? lastScoreNorth : lastScoreSouth);

                if (north)
                    lastScoreNorth = nextScore;
                else
                    lastScoreSouth = nextScore;

                temp.addScore(newScore);
                priorityQueueForEachMov.Dequeue().ChangePlace(temp);
            }

            lastMovesToScore.Enqueue(move);
            priorityQueueForEachMov.Enqueue(queue);

        }

        public static void NewGame()
        {
            foreach (MoveToScoreManager move in lastMovesToScore)
            {
                float nextScore = Leader.getCurrentScore(move.North);
                move.addScore(nextScore);
                priorityQueueForEachMov.Dequeue().ChangePlace(move);
            }

            lastMovesToScore = new Queue<MoveToScoreManager>();
            priorityQueueForEachMov = new Queue<PriorityQueue<MoveToScoreManager>>();
            lastScoreNorth = 0;
            lastScoreSouth = 0;

        }

        #endregion

        #region Get The Next Way
        private static readonly int MAX_NUMBER_OF_PLACES_CAN_GO = 4;
        public List<BlockPlayerContainer> getMovesNeeded(List<int> placeCanGoTo , bool north)
        {

            MoveToScoreManager toReturn = null;
            
            placeCanGoTo.Sort();

            if (placeCanGoTo.Count > MAX_NUMBER_OF_PLACES_CAN_GO)
            {
                List<int> temp = new List<int>();
                for (int i = 0; i < MAX_NUMBER_OF_PLACES_CAN_GO; i++)
                    if (north)
                        temp.Add(placeCanGoTo[placeCanGoTo.Count - MAX_NUMBER_OF_PLACES_CAN_GO + i]);
                    else
                        temp.Add(placeCanGoTo[i]);
                placeCanGoTo = temp;
            }

            if (allMovesAndScores.Count < 10)
                toReturn = getRandomOrder(placeCanGoTo);
            else
            {
                if (getRandomNumber(100) < 90)
                {
                    MoveToScoreManager peek = allMovesAndScores.top();
                    if (peek.Score <= 0)
                    {
                        if(!peek.isRealaiable)
                            toReturn = peek;
                        else
                            toReturn = getRandomOrder(placeCanGoTo);
                    }
                    else
                    {
                        toReturn = peek;
                    }
                }
                else
                {
                    MoveToScoreManager peek;
                    int j = getRandomNumber(allMovesAndScores.Count);
                    for (int i = 0; i < allMovesAndScores.Count; i++)
                    {
                        peek = allMovesAndScores.getNearTheHeigher(j % allMovesAndScores.Count);
                        if (peek.Score <= 0)
                        {
                            if (!peek.isRealaiable)
                            {
                                toReturn = peek;
                                break;
                            }
                        }
                        else
                        {
                            toReturn = peek;
                            break;
                        }
                            
                    }

                }
            }

            if(toReturn == null)
                toReturn = getRandomOrder(placeCanGoTo);

            setScore(toReturn , allMovesAndScores , north);
            
            allMovesAndScores.push(toReturn);

            return toReturn.Moves;

        }
        #endregion

        #region Random Functions and helpers
        private MoveToScoreManager getRandomOrder(List<int> placeCanGoTo)
        {
            List<BlockPlayerContainer> lst = new List<BlockPlayerContainer>();
            for (int i = 0; i < placeCanGoTo.Count; i++)
            {
                lst.Add(new BlockPlayerContainer(placeCanGoTo[i], 0));
            }

            for (int i = 0; i < Leader.MAX_NUMBER_OF_SOLDIERS; i++)
                lst[getRandomNumber(placeCanGoTo.Count - 1)].AddAnotherPlayer();

            return new MoveToScoreManager(lst);
        }

        private static System.Random rand = new System.Random();
        public static int getRandomNumber(int max , int min = 0)
        {
            if (min > max)
                throw new System.Exception("You dickHead");
            try
            {
                double d = rand.NextDouble();
                return (((int)(d * int.MaxValue)) % (max - min + 1)) + min;
            }
            catch
            {
                throw new Exception("Gay!!!!!");
            }
        }
        #endregion

    }


}
