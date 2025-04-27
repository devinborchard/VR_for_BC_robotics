using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

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
    public GameObject handRotationRef;
    public GameObject locationRef;

    private string simDir = "c:/Users/Devin/Desktop/demos/dataStackv2.hdf5";
    public string demoName = "2025-04-22_08-46-26_30861";

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
    void Awake()
    {
        totalTime = 0;

        episodeController.GetComponent<StateRecorder>().simulation = true;    
        robot.GetComponent<InputController>().simulation = true;

        simulationInterval = 1f / 20f;

        
        GameData.randomSeed = Int32.Parse(demoName.Split('_')[2]);

        RunPythonScript("C:/Users/Devin/CS933 Project/Assets/PythonScripts/findAction.py", simDir, demoName);
    }

    void RunPythonScript(string scriptName, params string[] args)
    {
        string pythonPath = "python"; // Or full path like "C:/Python39/python.exe"
        string scriptPath = Path.Combine(Application.dataPath, "Scripts", scriptName);


        string arguments = $"\"{scriptPath}\"";
        foreach (string arg in args)
        {
            arguments += $" \"{arg}\""; // Add quotes in case args have spaces
        }

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = arguments;
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;
        string output;
        using (Process process = Process.Start(start))
        {
            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

        }
        UnityEngine.Debug.Log("Python Output:\n" + output);
        ParseOutput(output);
    }

    void ParseOutput(string input)
    {
        var result = new List<List<float>>();

        // Split into lines
        string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var row = new List<float>();
            string[] values = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string val in values)
            {
                if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
                {
                    row.Add(parsed);
                }
            }

            result.Add(row);
        }

        actions = result;
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

        if(dgrip == 1){
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

        // Calculate target position and rotation
        targetPosition = startPosition + new Vector3(dx, dy, dz);

        // Reset time for interpolation
        elapsedTime = 0f;
        isMoving = true;
    }

    

    void Update()
    {
        if (step < actions.Count - 1)
        {
            
            

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

            // Trigger movement at the next step
            if (timer >= simulationInterval)
            {
                Simulate(step);
                step++;
                timer = 0f;
            }
        }
    }
}
