using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowConsole : MonoBehaviour {
	public GameObject consoleLog;
	private bool isShowing;

	void awake(){
		isShowing = false;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp ("`")) {
			isShowing = !isShowing;
			consoleLog.SetActive (isShowing);
		}
	}
}
