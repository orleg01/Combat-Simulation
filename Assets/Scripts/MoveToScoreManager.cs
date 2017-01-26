using UnityEngine;
using System.Collections.Generic;

public class MoveToScoreManager {

    private List<BlockPlayerContainer> moves;
    private int numberAddedItem = 0;
    private float score;
    private static int ID_GENERATOR = 100;
    private int myId;



    private static readonly int LIMIT_FOR_REALAIABLE_DATA = 50;
    public float Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
        }
    }
    public bool isRealaiable
    {
        get
        {
            return numberAddedItem > LIMIT_FOR_REALAIABLE_DATA;
        }
    }
    public float GetComperatorHelper
    {
        get
        {
            return 1 / myId;
        }
    }
    public List<BlockPlayerContainer> Moves
    {
        get
        {
            return moves;
        }
    }
    public bool North
    {
        get;
        set;
    }

    public MoveToScoreManager(List<BlockPlayerContainer> moves, float score = 0)
    {
        myId = ID_GENERATOR++;
        Score = score;
        this.moves = moves;
    }
    public bool movesTheSame(List<BlockPlayerContainer>  list)
    {

        if (list.Count == moves.Count)
        {
            for (int i = 0; i < moves.Count; i++)
                if (moves[i] != list[i])
                    return false;
            return true;
        }

        return false;
    }

    public bool movesTheSame(MoveToScoreManager other)
    {
        return movesTheSame(other.Moves);
    }
    public void addScore(float score)
    {
        numberAddedItem++;
        Score = (int)(Score * 0.9 + score * 0.1);
    }

}
