using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class StateRecorder : MonoBehaviour
{
    public GameObject location;

    public GameObject robot;
    public float logInterval = 1f / 20f; // 20 Hz (every 0.033s)
    private float timer = 0f;
    public string basePath;
    private string filePath;

    private Quaternion startRotationQ;
    private Vector3 startRotation;

    public float totalTime;
    public bool demoStarted;
    public bool done;
    private float timeDone = 0;
    private bool isVR;
    private string demoName;

    public bool simulation = false;
    public Camera agentCam;
    public Camera outerCam;

    private string currentDateTime;

    // Start is called before the first frame update
    void Start()
    {
        isVR = SceneManager.GetActiveScene().name.Contains("VR");
        startRotation = location.transform.rotation.eulerAngles;
        startRotationQ = location.transform.rotation;
        currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        demoName = $"{currentDateTime}_{GameData.randomSeed}";
        filePath = $"{basePath}/{demoName}/demo.txt";
        totalTime = 0;
        Debug.Log($"Writing to file: {filePath}");

        RenderTexture rt = new RenderTexture(84, 84, 24);
        agentCam.targetTexture = rt;
        outerCam.targetTexture = rt;

        Directory.CreateDirectory(Path.Combine(basePath, demoName));
        Directory.CreateDirectory(Path.Combine(basePath, demoName, "overCaptures"));
        Directory.CreateDirectory(Path.Combine(basePath, demoName, "handCaptures"));

        done = false;
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

    void CamCapture(Camera camera, string path){

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        camera.Render();

        Texture2D Image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);

        File.WriteAllBytes(basePath + "/" + demoName + "/"+path+"/"  + totalTime + ".png", Bytes);
    }

    void CaptureState(){
        Vector3 p = location.transform.position;
        Vector3 r = location.transform.rotation.eulerAngles - startRotation;
        Quaternion q = location.transform.rotation * Quaternion.Inverse(startRotationQ);

        bool gripping;
        if(isVR){
            gripping = robot.GetComponent<InputControllerVR>().gripping;
        }
        else{
            gripping = robot.GetComponent<InputController>().gripping;
        }

        int finished = 0;
        if(done){finished = 1;}
        
        int grippedState = 0;
        if(gripping){grippedState = 1;}

        string robotState = $"X: {p.x:F4}, Y: {p.y:F4}, Z: {p.z:F4}, QX: {q.x:F4}, QY: {q.y:F4}, QZ: {q.z:F4}, QW: {q.w}, RX:{r.x}, RY:{r.y}, RZ:{r.z}, G: {grippedState}";
        string stateString = $"T: {totalTime}, {robotState}, D: {finished}\n";

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
                CamCapture(agentCam, "handCaptures");
                CamCapture(outerCam, "overCaptures");
                timer = 0f; // Reset timer
            }
        }

        if(done){
            timeDone += Time.deltaTime;
            if(timeDone > 1){
                gameObject.GetComponent<EpisodeController>().EndEpisode();       
            }
        }
    }
}
