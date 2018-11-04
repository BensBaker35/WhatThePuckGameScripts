using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {

	public void SwitchToGameScene()
    {
        
        SceneManager.LoadScene(1);
        Debug.Log(SceneManager.sceneCount);
    }
}
