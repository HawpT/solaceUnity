using UnityEngine;
using solace;
using System.Collections.Generic;

public class DefaultCharacterBehavior : MonoBehaviour {
    private Character charObject;
    private Ray ray;
    private RaycastHit hit;
    public string characterName;
    public static Dictionary<string,Character> allCharacters;
    public static Dictionary<string, Dictionary<string, int>> core;
    static DefaultCharacterBehavior()
    {
        allCharacters = new Dictionary<string, Character>();
    }

    // Use this for initialization
    void Start () {
        core = new Dictionary<string, Dictionary<string, int>>();
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {

    }
}
