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
    public float logInterval = 1f / 20f; // 20 Hz (every 0.033s)
    private float timer = 0f;

    private string filePath;

    private Vector3 startRotation;

    public float totalTime;
    public bool demoStarted;
    private bool isVR;

    public bool simulation = false;
    public Camera agentCam;

    private string currentDateTime;


    // Start is called before the first frame update
    void Start()
    {
        isVR = SceneManager.GetActiveScene().name.Contains("VR");
        startRotation = location.transform.rotation.eulerAngles;
        currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        filePath = Application.persistentDataPath + $"/{currentDateTime}/demo.txt";
        totalTime = 0;
        Debug.Log($"Writing to file: {filePath}");

        if (agentCam.targetTexture == null)
        {
            RenderTexture rt = new RenderTexture(256, 256, 24);
            agentCam.targetTexture = rt;
        }
        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, currentDateTime));
        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, currentDateTime, "camCaptures"));

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

    void CamCapture(){

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = agentCam.targetTexture;

        agentCam.Render();

        Texture2D Image = new Texture2D(agentCam.targetTexture.width, agentCam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, agentCam.targetTexture.width, agentCam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);

        File.WriteAllBytes(Application.persistentDataPath + "/" + currentDateTime + "/camCaptures/"  + totalTime + ".png", Bytes);
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
        string stateString = $"T: {totalTime}, {robotState}\n";

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
                CamCapture();
                timer = 0f; // Reset timer
            }
        }
    }
}
