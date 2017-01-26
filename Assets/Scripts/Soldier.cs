using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Soldier : MonoBehaviour {

    public GameObject Explotion;
    public GameObject ExplotionLocator;
    public GameObject CameraHolder;

    public Animation anime;
    
    public bool DEBUG;
    public GameObject forDebug;

    private Vector3 goTo;
    private static List<BoxCollider> colliders;

    private bool goingNorth;
    private Leader leader;

    public int Health;

    public int NumberOfBlockIn
    {
        get;
        set;
    }

    private Vector3 learnModePos;
    public Vector3 Position
    {
        get
        {
            if (Leader.LearnMode)
            {
                return learnModePos;
            }
            return gameObject.transform.position;
        }
        set
        {
            if (Leader.LearnMode)
            {
                learnModePos = value;
                return;
            }
            gameObject.transform.position = value;
        }
    }
    public void FixPosAfterLearning()
    {
        transform.position = learnModePos;
    }

    public float LenToGo
    {
        get
        {
            return Vector3.Distance(Position , goTo);
        }
    }
    public Vector3 LearnMode_WhereGoingTo
    {
        get
        {
            return goTo;
        }
    }

    public void LearnMode_Start()
    {
        Health = 100;
    }
    // Use this for initialization
    void Start () {
        anime = GetComponent<Animation>();
        Health = 100;
    }
    public static void SetColliders(List<BoxCollider> lst)
    {
        colliders = lst;
    }
    public void Initilaize( bool north , Leader lead)
    {
        goingNorth = north;
        leader = lead;
    }

    private List<Vector3> myWay;
    private Vector3 lastPos;
    private Vector3 lookat;
    public static float factorForSoldierVelocity = 8;
    public static readonly float DISTANSE_FACTOR_FOR_WALK = 0.2f;
    public float timeToShoot = 0.5f;
    private bool died = false;
    private float time;
    public static readonly float RADIUS_OF_CIRCLE = 10;
    public static readonly float HOW_MUCH_REDUSE_VELOCITY_OF_CAMERA = 5;
    void FixedUpdate ()
    {
        time += Time.fixedDeltaTime / HOW_MUCH_REDUSE_VELOCITY_OF_CAMERA;
        CameraHolder.transform.position = Position +  new Vector3((float)Math.Cos(time) , 0.6f, (float)Math.Sin(time) ) * RADIUS_OF_CIRCLE;
        CameraHolder.transform.LookAt(Position + new Vector3(0 , 1 , 0));

        if (died)
            return;
        if (myWay != null && myWay.Count >= 1)
        {
            /*if (!anime.GetBool("Walking"))
                anime.SetBool("Walking" , true); */
            anime.CrossFade(AnimatorHelper.run);
            lookat = myWay[0];
            lookat.y = 0;
            transform.LookAt(lookat);

            if (Vector3.Distance(Position, myWay[0]) < DISTANSE_FACTOR_FOR_WALK)
            {
                myWay.RemoveAt(0);
                return;
            }
            Vector3 dir = myWay[0] - Position;

            dir = dir.normalized * factorForSoldierVelocity;
            dir.y = 0;
            lastPos = Position;
            Position += (dir * Time.fixedDeltaTime);

            if (lastPos == Position)
                myWay.RemoveAt(0);

        }
        else
        {
            /*if (anime.GetBool("Walking"))
                anime.SetBool("Walking", false);*/

            crouchAnimationOn();

            if (NumberOfBlockIn == 0 || NumberOfBlockIn == colliders.Count - 1)
                return;

            timeToShoot -= Time.fixedDeltaTime;
            if (timeToShoot < 0)
            {
                /*anime.SetBool("Fire" , true);*/
                //anime.CrossFade(AnimatorHelper.fire);

                Instantiate(Explotion, ExplotionLocator.transform.position, Quaternion.identity);

                if (NumberOfBlockIn == 0 || NumberOfBlockIn == colliders.Count - 1)
                    return;
                Soldier nearerEnemySoldier = leader.GetNearerEnemySoldier(this);
                if (nearerEnemySoldier == null)
                {
                    timeToShoot = 0.5f;
                    return;
                }
                transform.LookAt(nearerEnemySoldier.transform);
                nearerEnemySoldier.Shoot(Vector3.Distance(Position , nearerEnemySoldier.Position));

                timeToShoot = 0.5f + (DesitionMaker.OptionToGo.getRandomNumber(5) / 5f);
                //Invoke("crouchAnimationOn", 3f);
            }
        }

        if (myWay != null && myWay.Count == 0)
            myWay = null;
        
	}

    public void crouchAnimationOn()
    {
        anime.CrossFade(AnimatorHelper.crouch);
    }

    #region Walking
    public void setDebugOn(GameObject obj)
    {
        DEBUG = true;
        forDebug = obj;
    }
    internal void GoTo(Vector3 whereToGoInWorld)
    {
        goTo = whereToGoInWorld;
        
        if (!Leader.LearnMode)
            findBestWayToGo();

        //if (Leader.LearnMode)
        //Position = whereToGoInWorld;
        //else
        //findBestWayToGo();
    }
    private void findBestWayToGo()
    {
        GameObject temp;
        int numberOfIteration = 1;
        
        #region fix endPoint

        while (!canGoThere(goTo, out temp))
        {
            float rotateDirection = temp.transform.rotation.y%90;
            Vector3 direction = (goTo - Position).normalized * numberOfIteration;
            Vector3 direcVectorSide = new Vector3( (float)Math.Sin(rotateDirection) , 1 , (float)Math.Cos(rotateDirection) );


            if (canGoThere(goTo + direcVectorSide))
                goTo += direcVectorSide;
            else if (canGoThere(goTo + direcVectorSide))
                goTo += direcVectorSide;
            else if (canGoThere(goTo - direction) && numberOfIteration == 1)
                goTo -= direction;

            numberOfIteration++;
        }

        #endregion
        
        Vector3 walker = new Vector3(Position.x , 0 , Position.z);
        myWay = new List<Vector3>();

        while (getDisstance(walker) > 1 )
        {

            Vector3 direction = (goTo - walker).normalized;
            if (canGoThere(direction + walker ,out temp))
            {
                myWay.Add(direction + walker);
                walker += direction;
            }
            else
            {
                Vector3 direcVectorSide = temp.transform.right;
                numberOfIteration = 1;
                
                BoxCollider boxCollider = temp.GetComponent<BoxCollider>();

                Vector3 centerOfObject = boxCollider.bounds.center;
                Vector3 boundOfObject = boxCollider.bounds.extents;


                if (centerOfObject.x - walker.x > 0)
                {
                    while (centerOfObject.x - walker.x < boundOfObject.x + 1)
                    {
                        walker -= direcVectorSide;
                        myWay.Add(walker);
                    }
                }
                else
                {
                    while (walker.x - centerOfObject.x < boundOfObject.x + 1)
                    {
                        walker += direcVectorSide;
                        myWay.Add(walker);
                    }
                }

                walker += temp.transform.forward*4 * (goTo.z - walker.z > 0 ? 1 : -1);
                myWay.Add(walker);
            }

            walker.y = 0;
        }

        numberOfIteration = 1;

        if(DEBUG)
            foreach (Vector3 vec in myWay)
            {
                Instantiate(forDebug, vec, Quaternion.identity).name = "Obj" + Time.realtimeSinceStartup + numberOfIteration++;
            }

    }

    private bool canGoThere(Vector3 vec)
    {
        GameObject temp;
        return canGoThere(vec, out temp);
    }
    private bool canGoThere(Vector3 vec, out GameObject touch)
    {
        foreach (BoxCollider boxCol in colliders)
            if (boxCol.bounds.Contains(vec))
            {
                touch = boxCol.gameObject;
                return false;
            }
        touch = colliders[0].gameObject;
        return true;
    }
    private float getDisstance(Vector3 vec)
    {
        return Vector3.Distance(vec, goTo);
    }
    #endregion

    #region Fight
    public bool Shoot(float distanse)
    {
        if (NumberOfBlockIn == 0 || NumberOfBlockIn == colliders.Count - 1)
            return false;

        int num = DesitionMaker.OptionToGo.getRandomNumber(60);
        float probability;

        probability = (8 / (distanse)) ;

        if (probability < 0)
            probability = 0;

        int anotherNum = DesitionMaker.OptionToGo.getRandomNumber((int)(1/probability));
        //if (goingNorth)
            //anotherNum = 1;
        if(anotherNum == 0)
            Health -= num;

        if (Health <= 0)
        {
            died = true;
            if (Leader.LearnMode)
            {
                Die();
            }
            else
            {
                anime.CrossFade(AnimatorHelper.die);
                Invoke("Die", 0.9f);
            }
            return true;
        }

        return false;
    }

    public void Die()
    {
        leader.SoldierDied(this);
    }
    #endregion

}
