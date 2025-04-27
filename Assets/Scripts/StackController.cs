using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StackController : MonoBehaviour
{
    public GameObject block1;
    public GameObject block2;

    private bool block1InZone;
    private bool block2InZone;
    public GameObject robot;
    private InputController inputController;
    private InputControllerVR inputControllerVr;

    private float stackedTimer = 0f;
    private float stackedTime = 1f;
    public GameObject episodeController;

    // Start is called before the first frame update

    void Start()
    {
        block1InZone = false;
        block2InZone = false;
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName.Contains("Basic")){
            inputController = robot.GetComponent<InputController>();
        }
        else{
            inputControllerVr = robot.GetComponent<InputControllerVR>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("grabbable")){
            if(other.gameObject.name == block1.name){
                block1InZone = true;
            }
            if(other.gameObject.name == block2.name){
                block2InZone = true;
            }
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("grabbable")){
            if(other.gameObject.name == block1.name){
                block1InZone = false;
            }
            if(other.gameObject.name == block2.name){
                block2InZone = false;
            }
        }    
    }

    bool CheckStacked(){
        bool gripping;

        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName.Contains("Basic")){
            gripping = inputController.gripping;

        }
        else if(currentSceneName.Contains("VR")){
            gripping = inputControllerVr.gripping;
        }
        else{
            gripping = robot.GetComponent<InputController>().gripping;
        }

        if(block1InZone && block2InZone && block2.transform.position.y - block1.transform.position.y > 0.01 && !gripping){
            return true;
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {   
        if(!GameData.isSim){
            if(stackedTimer >= stackedTime){
                episodeController.GetComponent<StateRecorder>().done = true;
            }
            bool stacked = CheckStacked();
            if(stacked){
                stackedTimer = stackedTimer + Time.deltaTime;
            }else{
                stackedTimer = 0;
            }
        } 
    }
}
