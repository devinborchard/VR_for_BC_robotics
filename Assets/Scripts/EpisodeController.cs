using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EpisodeController : MonoBehaviour
{
    // Start is called before the first frame update
    private StateRecorder stateRecorder;
    void Start()
    {
        stateRecorder = GetComponent<StateRecorder>();
    }

    public void EndEpisode(){
        // UnityEditor.EditorApplication.isPlaying = false;
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName.Contains("Basic")){
            SceneManager.LoadScene("BasicMenu");
        }
        else{
            SceneManager.LoadScene("VRMenu");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(stateRecorder.totalTime >= 30){
            EndEpisode();
        }
    }
}
