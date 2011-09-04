using UnityEngine;
using System.Collections;

public class SaveAsDestroyed : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameSaveSystem.SaveObjectState(gameObject.name,SaveObject.States.Destroyed);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
