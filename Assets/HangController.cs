using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangController : MonoBehaviour
{
    public GameObject mug;

    private bool mugInZone;
    public GameObject robot;
    private InputController inputController;

    private float hangTimer = 0f;
    private float hangTime = 2f;
    public GameObject episodeController;

    // Start is called before the first frame update

    void Start()
    {
        mugInZone = false;
        inputController = robot.GetComponent<InputController>();
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
        bool gripping = inputController.gripping;
        if(mugInZone && !gripping){
            return true;
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {   
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
