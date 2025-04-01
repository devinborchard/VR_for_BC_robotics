using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class StateRecorder : MonoBehaviour
{
    public GameObject location;

    public GameObject targetObject;

    public GameObject robot;
    public float logInterval = 1f / 30f; // 30 Hz (every 0.033s)
    private float timer = 0f;

    private string filePath;

    private Vector3 startRotation;

    public float totalTime;
    public bool demoStarted;
    private bool isVR;

    public bool simulation = false;

    // Start is called before the first frame update
    void Start()
    {
        isVR = SceneManager.GetActiveScene().name.Contains("VR");
        startRotation = location.transform.rotation.eulerAngles;
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        filePath = Application.persistentDataPath + $"/{currentDateTime}.txt";
        totalTime = 0;
        Debug.Log($"Writing to file: {filePath}");

        demoStarted = true;
        if(isVR){
            demoStarted = false;
        }
    }

    public void AppendToFile(string text)
    {   
        if(!simulation){
            File.AppendAllText(filePath, text);
        }
    }

    void CaptureState(){
        Vector3 p = location.transform.position;
        Vector3 r = location.transform.rotation.eulerAngles - startRotation;

        bool gripping;
        if(isVR){
            gripping = robot.GetComponent<InputControllerVR>().gripping;
        }
        else{
            gripping = robot.GetComponent<InputController>().gripping;
        }
        
        int grippedState = 0;
        if(gripping){grippedState = 1;}

        Vector3 targetP = targetObject.transform.position;
        Vector3 targetR = targetObject.transform.rotation.eulerAngles;
        string robotState = $"X: {p.x:F4}, Y: {p.y:F4}, Z: {p.z:F4}, RX: {r.x:F4}, RY: {r.y:F4}, RZ: {r.z:F4}, G: {grippedState}";
        string targetState = $"TX: {targetP.x:F4}, TY: {targetP.y:F4}, TZ: {targetP.z:F4}, TRX: {targetR.x:F4}, TRY: {targetR.y:F4}, TRZ: {targetR.z:F4}";
        string stateString = $"T: {totalTime}, {robotState}, {targetState}\n";

        AppendToFile(stateString);

        // Debug.Log($"X: {p.x:F4}, Y: {p.y:F4}, Z: {p.z:F4}, RX: {r.x:F4}, RY: {r.y:F4}, RZ: {r.z:F4}");
    }

    // Update is called once per frame
    void Update()
    {
        if(demoStarted && !simulation){
            totalTime += Time.deltaTime;
            timer += Time.deltaTime;
            if (timer >= logInterval){
                CaptureState();
                timer = 0f; // Reset timer
            }
        }
    }
}
