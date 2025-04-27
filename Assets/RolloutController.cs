using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks; // For async handling

public class RolloutController : MonoBehaviour
{

    private bool readyForStep = true;
    public GameObject EpisodeController;
    private StateRecorder stateRecorder;

    public GameObject robot;

    public GameObject locationRef;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    private float elapsedTime = 0f;
    private bool isMoving = false;
    private float simulationInterval;

    private float timer = 0f;
    public float totalTime;

    public GameObject hand;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log("START ROLLOUT");
        stateRecorder = EpisodeController.GetComponent<StateRecorder>();
        simulationInterval = 1f / 5f;
    }

    List<float> ParseOutput(string modelOutput){
        UnityEngine.Debug.Log($"RAW OUT {modelOutput}");
        int startIndex = modelOutput.IndexOf('[');
        int endIndex = modelOutput.IndexOf(']');
        string input = modelOutput.Substring(startIndex + 1, endIndex - startIndex - 1);

        // Split the string by commas
        string[] stringValues = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Convert each value to a float and store in a list
        List<float> floatValues = new List<float>();
        foreach (string str in stringValues)
        {  
            floatValues.Add(float.Parse(str.Trim()));  // Trim to remove extra spaces if any
        }
        return floatValues;
    }

    string GetState(){
        string state = stateRecorder.CaptureState();


        byte[] image1 = stateRecorder.CamCapture(stateRecorder.agentCam ,"");
        byte[] image2 = stateRecorder.CamCapture(stateRecorder.outerCam ,"");
        
        string base64Image1 = Convert.ToBase64String(image1);
        string base64Image2 = Convert.ToBase64String(image2);
        
        return state.Replace(" ","")+ "~" + base64Image1 + "~" + base64Image2 + "!";
    }

    void DoAction(List<float> action){

        UnityEngine.Debug.Log("Doing: " + string.Join(", ", action));

        float dx = action[0];
        float dy = action[1];
        float dz = action[2];

        float drx = action[3];
        float dry = action[4];
        float drz = action[5];

        // UnityEngine.Debug.Log("AC: " + dx + ", " + dy + ", " + dz + ", " + drx + ", " + dry + ", " + drz);

        float dgrip = action[6];


         if(dgrip > 0.8f){
            robot.GetComponent<InputController>().gripping = true;
        }
        else{
            robot.GetComponent<InputController>().gripping = false;
        }

        // Store start position and rotation
        startPosition = locationRef.transform.position;
        // startRotation = handRotationRef.transform.rotation;
        startRotation = locationRef.transform.rotation;

        // Apply rotation delta to child's world rotation to get target
        Quaternion rotationDelta = Quaternion.Euler(-drx, dry, drz);
        targetRotation = rotationDelta * startRotation;

        // // Calculate target position and rotation
        // Vector3 localDelta = new Vector3(dx, dy, dz);
        // Vector3 worldDelta = locationRef.transform.TransformDirection(localDelta);
        targetPosition = startPosition + new Vector3(dx, dy, dz);


        // Reset time for interpolation
        elapsedTime = 0f;
        isMoving = true;
    }
    
    void Update()
    {
        // elapsedTime += Time.deltaTime

        totalTime += Time.deltaTime;
        timer += Time.deltaTime;

        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / simulationInterval); // Normalize t between 0 and 1

            // Interpolate position and rotation over time
            // Compute desired child world position at this timestep
            Vector3 interpolatedChildPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // Compute how to move the parent so the child ends up in the right spot
            Vector3 positionOffset = interpolatedChildPosition - locationRef.transform.position;
            hand.transform.position += positionOffset;
            
            // Get current rotation of the child
            Quaternion currentChildRotation = locationRef.transform.rotation;

            // Interpolate desired child world rotation
            Quaternion interpolatedChildRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Compute how we need to rotate the parent to make child match interpolated rotation
            Quaternion deltaToApply = interpolatedChildRotation * Quaternion.Inverse(currentChildRotation);
            hand.transform.rotation = deltaToApply * hand.transform.rotation;

            // Stop movement when finished
            if (t >= 1f)
            {
                isMoving = false;
            }
        }

        if(timer >= simulationInterval){
            // UnityEngine.Debug.Log($"UPDATING: {timer}");
            timer = 0;

            string stateWithImages = GetState();
            string action = gameObject.GetComponent<PythonConnection>().SendStateToPython(stateWithImages);
            List<float> parsedAction = ParseOutput(action);

            DoAction(parsedAction);
        }
        
    }
}
