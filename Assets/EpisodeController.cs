using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeController : MonoBehaviour
{
    // Start is called before the first frame update
    private StateRecorder stateRecorder;
    void Start()
    {
        stateRecorder = GetComponent<StateRecorder>();
    }

    public void EndEpisode(bool status){
        if(status){
            stateRecorder.AppendToFile("success");
        }else{
            stateRecorder.AppendToFile("fail");
        }
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(stateRecorder.totalTime >= 30){
            EndEpisode(false);
        }
    }
}
