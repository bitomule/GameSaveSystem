using UnityEngine;
using System;
using System.Collections;

public class LoadScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameSaveSystem.SetUser(0,"NameExample");
		GameSaveSystem.LoadLevel();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
