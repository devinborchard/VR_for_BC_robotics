using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class GameData
{
    public static int randomSeed = Random.Range(1, 100000);
    public static void SetNewSeed(){
        randomSeed = Random.Range(1, 100000);
    }
    // public static bool isSim = SceneManager.GetActiveScene().name.Contains("Simulation");
    public static bool isSim = false;
}
