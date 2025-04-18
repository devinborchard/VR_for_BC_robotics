using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Start is called before the first frame update

    private string type;
    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName.Contains("Basic")){
            type = "Basic";
        }
        else{
            type = "VR";
        }
    }

    // Update is called once per frame
    void Update()
    {
        string sceneToLoad = "";
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            sceneToLoad = $"Square{type}";
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            sceneToLoad = $"Stack{type}";
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            sceneToLoad = $"Mug{type}";
        }

        if(sceneToLoad != ""){
            GameData.SetNewSeed();
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
