using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;



public class VRHandGrabbing : MonoBehaviour
{
    private bool overlappingHand;
    private bool usingHand;
    private bool gripping = true;

    public GameObject visualController;

    public GameObject hand;

    public GameObject episodeController;
   
    [SerializeField]
    XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");

    // Start is called before the first frame update
    void Start()
    {
        overlappingHand = false;
        usingHand = false;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("roboHand")){
            overlappingHand = true;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("roboHand")){
            overlappingHand = false;
        }
    }

    void DetectGrab(){

        if (m_GripInput == null)
        {
            Debug.LogWarning("Grip Input is not assigned!");
            return;
        }

        var gripVal = m_GripInput.ReadValue();
        if (gripVal > 0.5f)
        {
            gripping = true;
        }
        else{
            gripping = false;
        }
    }

    void UseHand(){
        usingHand = true;
        hand.transform.SetParent(transform);
        hand.GetComponent<Rigidbody>().isKinematic = true;
        visualController.SetActive(false);
        episodeController.GetComponent<StateRecorder>().demoStarted = true;
    }

    void StopUsingHand(){
        usingHand = false;
        hand.transform.SetParent(null);
        hand.GetComponent<Rigidbody>().isKinematic = false;
        visualController.SetActive(true);
    }

    void GrabbingLogic(){
        if(!usingHand && overlappingHand && gripping){
            UseHand();
        }
        else if(usingHand && overlappingHand && !gripping){
            StopUsingHand();
        }
    }

    // Update is called once per frame
    void Update()
    {
        DetectGrab();
        GrabbingLogic();
    }
}
