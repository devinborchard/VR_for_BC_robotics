using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectGrippingController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject location;
    public GameObject finger1Pad;
    public GameObject finger2Pad;

    private DetectGripped detectGripped1;
    private DetectGripped detectGripped2;

    private GameObject grabbedObject;
    private bool isVR;

    void Start()
    {
        isVR = SceneManager.GetActiveScene().name.Contains("VR");

        detectGripped1 = finger1Pad.GetComponent<DetectGripped>();
        detectGripped2 = finger2Pad.GetComponent<DetectGripped>();
    }

    // Update is called once per frame
    void Update()
    { 
        bool gripped1 = detectGripped1.gripped;
        bool gripped2 = detectGripped2.gripped;
        bool gripped = false;
        if(gripped1 && gripped2 && !gripped){
            if(detectGripped1.grippedObject.name == detectGripped2.grippedObject.name){
                gripped = true;
                grabbedObject = detectGripped1.grippedObject;
                grabbedObject.GetComponent<GrabbableController>().SetGrabbed(location.transform);
            }
        }

        if(!gripped && grabbedObject != null){
            grabbedObject.GetComponent<GrabbableController>().SetUnGrabbed();
        }

        if(isVR){
            GetComponent<InputControllerVR>().gripped = gripped;
        }
        else{
            GetComponent<InputController>().gripped = gripped;
        }
    }
}
