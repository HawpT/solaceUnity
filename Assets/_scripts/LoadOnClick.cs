using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadOnClick : MonoBehaviour {

	
    //changes to a given game scene
    public void LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }
}
