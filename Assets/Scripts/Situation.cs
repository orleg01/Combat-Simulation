using UnityEngine;
using System.Collections;

public class Situation {

    public int CurrentPlayerInBlock
    {
        get;
        set;
    }
    public BlockInformationContainer.WhoHaveTheBlock BlockConqureBy_GoingDirection
    {
        get;
        set;
    }


    public Situation(int currentPlayerInBlock, BlockInformationContainer.WhoHaveTheBlock blockConqureBy_GoingDirection)
    {
        CurrentPlayerInBlock = currentPlayerInBlock;
        BlockConqureBy_GoingDirection = blockConqureBy_GoingDirection;
    }
}
