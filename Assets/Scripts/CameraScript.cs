using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public GameObject cameraViewFromAbove;

    void Start () {
        tempLookAt = cameraViewFromAbove;
	}

    // Update is called once per frame
    private GameObject tempLookAt;
    private int soldierLookingAt = 0;
	void Update () {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            soldierLookingAt = soldierLookingAt % (Leader.goingNorthSoldier.Count + Leader.goingSouthSoldier.Count);

            if (Leader.goingSouthSoldier.Count > soldierLookingAt)
                tempLookAt = Leader.goingSouthSoldier[soldierLookingAt].CameraHolder;
            else
                tempLookAt = Leader.goingNorthSoldier[soldierLookingAt - Leader.goingSouthSoldier.Count].CameraHolder;
            
            soldierLookingAt++;
        }

        if (Input.GetKeyUp(KeyCode.U))
        {
            tempLookAt = cameraViewFromAbove;
        }

        try
        {
            transform.position = tempLookAt.transform.position;
            transform.rotation = tempLookAt.transform.rotation;
        }
        catch { }


    }
}
