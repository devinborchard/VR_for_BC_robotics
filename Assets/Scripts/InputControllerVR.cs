using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class InputControllerVR : MonoBehaviour
{
   // Start is called before the first frame update
    public float gripSpeed = 0.075f;
    public GameObject finger1;
    public GameObject finger2;
    public bool gripping;

    public bool gripped; //set elsewhere in DetectGripped

    [SerializeField]
    XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");


    void Start()
    {
        gripping = false;
        gripped = false;
    }


    void DetectGripperInput(){
        if (m_TriggerInput == null)
        {
            Debug.LogWarning("Grip Input is not assigned!");
            return;
        }

        var gripVal = m_TriggerInput.ReadValue();
        if (gripVal > 0.5f)
        {
            gripping = true;
        }
        else{
            gripping = false;
        }
    }


    void UpdateFingers(){
        bool closing = gripping;
        bool opening = !gripping;
        if((closing && finger1.transform.localPosition.y > 0 && !gripped) || (opening && finger1.transform.localPosition.y < 0.03f)){

            // Debug.Log($"Opening: {opening}, Closing: {closing}");
            int direction = 0;

            Vector3 gripperAxis = new Vector3(0, 1, 0);

            if(opening) direction = 1;
            if(closing) direction = -1;
            if(opening || closing){
                finger1.transform.localPosition += gripperAxis * gripSpeed * Time.deltaTime * direction;
                finger2.transform.localPosition += gripperAxis * gripSpeed * Time.deltaTime * direction * -1;
            }

        }

        
    }


    // Update is called once per frame
    void Update()
    {
        DetectGripperInput();
        UpdateFingers();
    }
}
