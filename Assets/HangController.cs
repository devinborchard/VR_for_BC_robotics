using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HangController : MonoBehaviour
{
    public GameObject mug;

    private bool mugInZone;
    public GameObject robot;
    private InputController inputController;
    private InputControllerVR inputControllerVr;

    private float hangTimer = 0f;
    private float hangTime = 2f;
    public GameObject episodeController;

    // Start is called before the first frame update

    void Start()
    {
        mugInZone = false;
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
            if(other.gameObject.name == mug.name){
                mugInZone = true;
            }
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("grabbable")){
            if(other.gameObject.name == mug.name){
                mugInZone = false;
            }
        }    
    }

    bool CheckHung(){
         bool gripping;

        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName.Contains("Basic")){
            gripping = inputController.gripping;
        }else{
            gripping = inputControllerVr.gripping;
        }

        if(mugInZone && !gripping){
            return true;
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {   
        if(!GameData.isSim){
            if(hangTimer >= hangTime){
                episodeController.GetComponent<StateRecorder>().done = true;
            }
            bool hung = CheckHung();
            if(hung){
                hangTimer = hangTimer + Time.deltaTime;
            }else{
                hangTimer = 0;
            }
        }
        
    }
}
