using UnityEngine;
using System.Collections;

public class Killer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DestroyObject(this, 1f);
	}
	
}
