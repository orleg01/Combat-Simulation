using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Leader : MonoBehaviour {

    //public GameObject cameraViewFromAbove;
    public GameObject soldierPrefab;
    public GameObject[] areas;
    public Vector3 spawnArea;
    public int RADIUS_FOR_SPAWN = 5;
    public bool goingNorth;

    private static List<BoxCollider> colliders;
    private List<Soldier> soldiers;
    private static DesitionMaker BigGod;

    public bool DEBUG;
    public GameObject forDebug;

    private static MyList mapOfWalking;

    public static List<Soldier> goingNorthSoldier;
    public static List<Soldier> goingSouthSoldier;

    private static bool learnMode; 
    public static bool LearnMode
    {
        get
        {
            return learnMode;
        }
        set
        {
            learnMode = value;
            if (learnMode)
            {
                foreach (Soldier sol in goingNorthSoldier)
                {
                    sol.Position = sol.transform.position;
                    DestroyObject(sol.gameObject);
                }
                foreach (Soldier sol in goingSouthSoldier)
                {
                    sol.Position = sol.transform.position;
                    DestroyObject(sol.gameObject);
                }
            }
           /* else
            {
                foreach (Soldier sol in goingNorthSoldier)
                {
                    sol.FixPosAfterLearning();
                }
                foreach (Soldier sol in goingSouthSoldier)
                {
                    sol.FixPosAfterLearning();
                }
            }*/
        }
    }

    public bool GameFinish
    {
        get
        {
            return mapOfWalking.GameIsFinish;
        }
    }

	// Use this for initialization
	void Start () {

        soldiers = new List<Soldier>();

        if (goingNorth)
        {

            goingNorthSoldier = new List<Soldier>();
            goingSouthSoldier = new List<Soldier>();

            colliders = new List<BoxCollider>();

            foreach (GameObject obj in areas)
                colliders.AddRange(obj.GetComponentsInChildren<BoxCollider>());

            Soldier.SetColliders(colliders);
            

            InitializePointerForBoldier();
            

            BigGod = new DesitionMaker();

            gameStart = true;

            /*if (DEBUG)
            { 
                int helperForPrint = 1;
                foreach (BlockInformationContainer blockInfo in startsPointsForBoldier)
                {
                    List<Vector3> goingNorthLst = blockInfo.GetBlocksToStandForSoldiers_ForDebug(true);
                    List<Vector3> goingSouthLst = blockInfo.GetBlocksToStandForSoldiers_ForDebug(false);

                    foreach (Vector3 vec3North in goingNorthLst)
                        Instantiate(forDebug, vec3North, Quaternion.identity).name = "stand" + "_North_" + helperForPrint++;
                    foreach (Vector3 vec3South in goingSouthLst)
                        Instantiate(forDebug, vec3South, Quaternion.identity).name = "stand" + "_South_" + helperForPrint++;

                }
            }*/

        }

        if (gameStart)
        {

           spawnNewSoldiers();
            //ChangeBoardOrder();
            //soldiers[soldiers.Count - 1].setDebugOn(forDebug);
        }

        if (goingNorth)
            goingNorthLeader = this;
        else
            goingSouthLeader = this;

    }

    internal Soldier GetNearerEnemySoldier(Soldier me)
    {
        List<Soldier> workWith = goingNorth ? goingSouthSoldier : goingNorthSoldier;

        if (workWith.Count == 0)
        {
            time = TIME_FOR_UPDATE;
            return null;
        }

        Soldier toReturn = workWith[0];

        float bestDistance = Vector3.Distance(toReturn.Position, me.Position);

        foreach (Soldier soldier in workWith)
        {
            if (soldier.NumberOfBlockIn == 0 || soldier.NumberOfBlockIn == colliders.Count - 1)
                continue;
            float helperDistanceSave;
            if ((helperDistanceSave = Vector3.Distance(soldier.Position, me.Position)) < bestDistance)
            {
                toReturn = soldier;
                bestDistance = helperDistanceSave;
            }
        }

        return toReturn;

    }


    // Update is called once per frame
    public static readonly int MAX_NUMBER_OF_SOLDIERS = 8;
    private static bool gameStart = false;
    public float time;
    private static readonly int TIME_FOR_UPDATE = 5;

    public static bool inClick = false;
    void Update () {
        time += Time.deltaTime;

        if (time > TIME_FOR_UPDATE)
        {
            time = 0;

            DoRound();
        }

        if (Input.GetKeyUp(KeyCode.W) && goingNorth)
        {
            if (!inClick)
            {
                inClick = true;
                //DestroyAllSoldiers();
                Time.timeScale = 0;
                /*StartCoroutine(*/LearnTheWayOfGame();//);
                Time.timeScale = 1;
            }
        }

	}

    private static bool needDoClearOnOtherPlayerAsWell = false;
    private void ClearGameIfNeeded(bool needToClear = false)
    {
        if (needDoClearOnOtherPlayerAsWell)
        {
            needDoClearOnOtherPlayerAsWell = false;
            soldiers = new List<Soldier>();
        }

        if (GameFinish || needToClear)
        {
            soldiers = new List<Soldier>();

            DesitionMaker.OptionToGo.NewGame();
            for (int i = goingNorthSoldier.Count - 1; i >= 0; i--)
            {
                Soldier sol = goingNorthSoldier[i];
                goingNorthSoldier.RemoveAt(i);
                if(!LearnMode)
                    DestroyObject(sol.gameObject);
            }

            for (int i = goingSouthSoldier.Count - 1; i >= 0; i--)
            {
                Soldier sol = goingSouthSoldier[i];
                goingSouthSoldier.RemoveAt(i);
                if(!LearnMode)
                    DestroyObject(sol.gameObject);
            }

            mapOfWalking.Clear();
            needDoClearOnOtherPlayerAsWell = true;
        }
    }

    private static System.Random random = new System.Random();
    public void spawnNewSoldiers(BlockInformationContainer info = null)
    {
        int counterNumberOfIter = 0;
        while (soldiers.Count < MAX_NUMBER_OF_SOLDIERS)
        {
            counterNumberOfIter++;
            float generateFactorX = ((float)(random.NextDouble() * 2) - 1) * RADIUS_FOR_SPAWN;
            float generateFactorZ = ((float)(random.NextDouble() * 2) - 1) * RADIUS_FOR_SPAWN;

            Soldier soldier;
            if (!LearnMode)
                soldier = (Instantiate(soldierPrefab, spawnArea + new Vector3(generateFactorX, 0, generateFactorZ), Quaternion.identity) as GameObject).GetComponent<Soldier>();
            else
            {
                soldier = new Soldier();
                soldier.LearnMode_Start();
            }
            soldier.Initilaize(goingNorth, this);
            soldiers.Add(soldier);
            (goingNorth ? goingNorthSoldier : goingSouthSoldier).Add(soldier);
            Vector3 goTo = mapOfWalking.AddNewSoldier(soldier, goingNorth , BlockInformationContainer.BoolToWhoHaveTheBlock(goingNorth));
            soldier.GoTo(goTo);
        }
        if (counterNumberOfIter == MAX_NUMBER_OF_SOLDIERS)
            ChangeBoardOrder();
    }

    internal void SoldierDied(Soldier soldier)
    {
        soldiers.Remove(soldier);
        if(!goingNorthSoldier.Remove(soldier))
            goingSouthSoldier.Remove(soldier);
        mapOfWalking.RemoveSoldier(soldier, soldier.NumberOfBlockIn, BlockInformationContainer.BoolToWhoHaveTheBlock(goingNorth));
        if(!LearnMode)
            DestroyObject(soldier.gameObject);
}

    private bool ChangeBoardOrder()
    {
        Situation[] sitInWorld = mapOfWalking.getSituationOfWorld();
        List<int> whereCanIGo = mapOfWalking.whereCanGo(BlockInformationContainer.BoolToWhoHaveTheBlock(goingNorth));

        List<BlockPlayerContainer> allPlayerContainer = BigGod.WhichWayShouldIChoose(sitInWorld, goingNorth, whereCanIGo);

        List<Soldier> tempSoldiers = new List<Soldier>(soldiers);

        foreach (Soldier soldier in soldiers)
        {
            foreach (BlockPlayerContainer blockPlayerContainer in allPlayerContainer)
            {
                if (soldier.NumberOfBlockIn == blockPlayerContainer.BlockNumber)
                {
                    if (blockPlayerContainer.HowMuchPeople > 0)
                    {
                        blockPlayerContainer.HowMuchPeople--;
                        tempSoldiers.Remove(soldier);
                        break;
                    }
                }
            }
        }

        foreach (Soldier soldier in tempSoldiers)
        {
            foreach (BlockPlayerContainer blockPlayer in allPlayerContainer)
            {
                if (blockPlayer.HowMuchPeople != 0)
                {
                    blockPlayer.HowMuchPeople--;
                    Vector3 whereToGoNext = mapOfWalking.ChangeLocation(soldier, goingNorth , blockPlayer.BlockNumber);
                    soldier.GoTo(whereToGoNext);
                }
            }
        }

        return true;
    }

    private void InitializePointerForBoldier( )
    {
        BoxCollider southBox;
        BoxCollider northBox;

        Dictionary<BoxCollider, BlockInformationContainer> dictToHelp = new Dictionary<BoxCollider, BlockInformationContainer>();

        southBox = northBox = colliders[0];

        foreach (BoxCollider colid in colliders)
        {

            if (colid.transform.position.z > northBox.transform.position.z)
                northBox = colid;
            else if (colid.transform.position.z < southBox.transform.position.z)
                southBox = colid;

            Vector3 center = colid.bounds.center;
            Vector3 limits = colid.bounds.extents;

            Vector3 placeToStayGoingNorth;
            Vector3 placeToStayGoingSouth;


            placeToStayGoingSouth = new Vector3(center.x - limits.x + 1f, 0, center.z + limits.z + 0.5f);
            placeToStayGoingNorth = new Vector3(center.x + limits.x - 1f, 0, center.z - limits.z - 0.5f);

            Vector3 directionOfBlock = colid.transform.right;

            dictToHelp.Add(colid , new BlockInformationContainer(
                                    (int)(limits.x * 2)   ,
                                    directionOfBlock      ,
                                    placeToStayGoingNorth ,
                                    placeToStayGoingSouth
                                ));

        }

        mapOfWalking = new MyList(northBox, dictToHelp[northBox], southBox, dictToHelp[southBox]);

        foreach (KeyValuePair<BoxCollider, BlockInformationContainer> keyVal in dictToHelp)
        {
            if (keyVal.Key != northBox && keyVal.Key != southBox)
            {
                mapOfWalking.addItem(keyVal.Key, keyVal.Value);
            }
        }

        mapOfWalking.GiveNumberToEachNode();

    }

    public static float getCurrentScore(bool goingNorth)
    {

        List<Soldier> workOn = goingNorth ? goingNorthSoldier : goingSouthSoldier;

        float toReturn = 0;

        foreach (Soldier soldier in workOn)
        {
            toReturn += goingNorth ? soldier.Position.z : 600 - soldier.Position.z; 
        }

        return toReturn;

    }

    private static int NUMBER_OF_ROUNDS = 35000;
    private static Leader goingNorthLeader = null;
    private static Leader goingSouthLeader = null;
    public static /*IEnumerator*/ void LearnTheWayOfGame()
    {
        LearnMode = true;
        int timeToUpdate = TIME_FOR_UPDATE * 2;

        for (int i = 0; i < NUMBER_OF_ROUNDS; i++)
        {

            if (i % 20000 == 0)
            {
                print(i + " Out of " + NUMBER_OF_ROUNDS);
               // Time.timeScale = 1;
               // yield return new WaitForSeconds(1);
             //   Time.timeScale = 0;
            //    print(i + " Out of " + NUMBER_OF_ROUNDS);
            }

            Leader first = i % 2 == 0 ? goingNorthLeader : goingSouthLeader;
            Leader sec   = i % 2 == 1 ? goingNorthLeader : goingSouthLeader; 

            first.DoRound();
            sec.DoRound();
            
            for (int j = 0; j < timeToUpdate; j++)
            {
                int index = j / 2;
                first.PlayInRound(index);
                first.PlayInRound(index);
            }


        }
        goingSouthLeader.ClearGameIfNeeded(true);
        goingNorthLeader.ClearGameIfNeeded();
        Time.timeScale = 1;
        LearnMode = false;
        inClick = false;
    }
    
    public void DoRound()
    {
        ClearGameIfNeeded();

        if (soldiers.Count > 0)
            ChangeBoardOrder();

        spawnNewSoldiers();
    }

    public void PlayInRound(int index)
    {
        foreach (Soldier sol in soldiers)
        {
            if (sol.LenToGo / Soldier.factorForSoldierVelocity >= index)
            {
                Soldier nearer = GetNearerEnemySoldier(sol);
                if (nearer == null)
                    return;
                nearer.Shoot(Vector3.Distance(sol.Position, nearer.Position));
            }
            sol.Position = sol.LearnMode_WhereGoingTo;
        }
    }

}
