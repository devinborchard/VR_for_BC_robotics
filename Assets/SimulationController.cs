using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;


public class SimulationController : MonoBehaviour
{
    // Start is called before the first frame update

    [Serializable]
    public class DataObject
    {
        public List<List<int>> states;
        public List<List<int>> actions;
    }

    public GameObject episodeController;
    public GameObject robot;
    public GameObject hand;

    private string simDir = "C:/Users/Devin/AppData/LocalLow/DefaultCompany/CS933_Project_Sim/";
    private string simFile = "2025-04-06_11-50-36-SIM";

    private List<List<float>> states;
    private List<List<float>> actions;

    private float simulationInterval;
    private float timer = 0f;
    public float totalTime;

    private int step = 0;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float elapsedTime = 0f;
    private bool isMoving = false;
    void Start()
    {
        totalTime = 0;

        episodeController.GetComponent<StateRecorder>().simulation = true;    
        robot.GetComponent<InputController>().simulation = true;

        simulationInterval =  episodeController.GetComponent<StateRecorder>().logInterval;

        string fileContents = File.ReadAllText(simDir+simFile+".txt");

        List<List<List<float>>> data = JsonConvert.DeserializeObject<List<List<List<float>>>>(fileContents);
        states = data[0];
        actions = data[1];

        Debug.Log(states.Count); // Pretty-print JSON
        Debug.Log(actions.Count); // Pretty-print JSON

    }


    void Simulate(int step)
    {
        if (step >= actions.Count) return; // Safety check

        List<float> action = actions[step];
        float dx = action[0];
        float dy = action[1];
        float dz = action[2];
        float drx = action[3];
        float dry = action[4];
        float drz = action[5];
        float dgrip = action[6];

        if(dgrip > 0){
            robot.GetComponent<InputController>().gripping = true;
        }
        if(dgrip < 0){
            robot.GetComponent<InputController>().gripping = false;
        }


        // Store start position and rotation
        startPosition = hand.transform.position;
        startRotation = hand.transform.rotation;

        // Calculate target position and rotation
        targetPosition = startPosition + new Vector3(dx, dy, dz);
        targetRotation = Quaternion.Euler(startRotation.eulerAngles + new Vector3(drx, dry, drz));

        // Reset time for interpolation
        elapsedTime = 0f;
        isMoving = true;
    }

    void Update()
    {
        if (step >= actions.Count - 1)
        {
            // episodeController.GetComponent<EpisodeController>().EndEpisode(true); // bool val doesn't matter here
            return;
        }

        totalTime += Time.deltaTime;
        timer += Time.deltaTime;

        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / simulationInterval); // Normalize t between 0 and 1

            // Interpolate position and rotation over time
            hand.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            hand.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Stop movement when finished
            if (t >= 1f)
            {
                isMoving = false;
            }
        }

        // Trigger movement at the next step
        if (timer >= simulationInterval)
        {
            Simulate(step);
            step++;
            timer = 0f;
        }
    }
}
