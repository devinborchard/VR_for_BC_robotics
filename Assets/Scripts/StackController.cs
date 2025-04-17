using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    public GameObject block1;
    public GameObject block2;

    private bool block1InZone;
    private bool block2InZone;
    public GameObject robot;
    private InputController inputController;

    private float stackedTimer = 0f;
    private float stackedTime = 3f;
    public GameObject episodeController;

    // Start is called before the first frame update

    void Start()
    {
        block1InZone = false;
        block2InZone = false;

        inputController = robot.GetComponent<InputController>();
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
        bool gripping = inputController.gripping;
        if(block1InZone && block2InZone && block2.transform.position.y - block1.transform.position.y > 0.01 && !gripping){
            return true;
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {   
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
